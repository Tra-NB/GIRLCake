<template>
  <div class="combo-page container py-5">
    <div class="header-box text-center mb-5">
      <h1 class="page-title">Combo ưu dãi ngọt ngào</h1>
      <p class="text-muted">Tiết kiệm hơn, trọn vẹn hương vị</p>
    </div>

    <div class="row g-4">
      <div class="col-md-4" v-for="item in combos" :key="item.id">
        
        <div 
          class="card combo-card h-100" 
          @click="goToDetail(item.id)" 
          :class="{'unavailable': !item.isAvailable}"
        >
          <div class="badge-wrapper">
             <span v-if="!item.isAvailable" class="badge-sold-out">HẾT HÀNG</span>
          </div>

          <div class="img-wrapper">
            <img 
              :src="resolveImage(item.imageUrl)" 
              :alt="item.name" 
              class="card-img-top" 
              @error="onImgError($event)" 
            />
          </div>
          
          <div class="card-body d-flex flex-column">
            <h5 class="card-title">{{ item.name }}</h5>
            
            <div class="rating-stars mb-2">
              <span v-for="n in 5" :key="n">
                <i class="bi" :class="{'bi-star-fill text-warning': n <= Math.round(item.avgRating), 'bi-star text-muted': n > Math.round(item.avgRating)}"></i>
              </span>
              <small class="ms-1 text-muted">({{ item.reviewCount }} đánh giá)</small>
            </div>

            <p class="card-text">{{ item.description || 'Ưu đãi combo hấp dẫn đang chờ bạn thưởng thức!' }}</p>
            
            <div class="mt-auto pt-3 border-top d-flex flex-column gap-2">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="text-muted small">Giá combo:</span>
                    <span class="price-tag">{{ formatPrice(item.price) }}</span>
                </div>

                <button 
                  @click.stop="addToCart(item.id, item.isAvailable)" 
                  class="btn btn-buy w-100"
                  :disabled="!item.isAvailable"
                >
                  <i class="bi bi-basket2-fill me-2"></i> Thêm vào giỏ
                </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import api from "@/api";
import "bootstrap-icons/font/bootstrap-icons.css";

/* =======================
   STATE
======================= */
const combos = ref([]);
const userToken = ref(null);
const userRole = ref("User");
const router = useRouter();

/* =======================
   USER SESSION
======================= */
const fetchUserSession = () => {
  const userData = localStorage.getItem("user");
  const token = localStorage.getItem("token");

  if (userData) {
    const user = JSON.parse(userData);
    userRole.value = user.role?.trim() || "User";
  }

  userToken.value = token;
};

/* =======================
   FETCH COMBOS
======================= */
const fetchCombos = async () => {
  try {
    const { data } = await api.get("/Combo/active");

    const mappedCombos = await Promise.all(
      data.map(async (c) => {
        const review = await fetchReviewsForCombo(c.id);

        return {
          id: c.id,
          name: c.name,
          description: c.description,
          price: c.price,
          imageUrl: c.imageUrl,
          isAvailable: c.isAvailable,
          avgRating: review.avgRating,
          reviewCount: review.reviewCount
        };
      })
    );

    combos.value = mappedCombos;
  } catch (error) {
    console.error("Lỗi load combo:", error);
  }
};

/* =======================
   FETCH REVIEWS
======================= */
const fetchReviewsForCombo = async (comboId) => {
  try {
    const { data } = await api.get(`/Reviews/product/${comboId}`);

    if (!data || data.length === 0) {
      return { avgRating: 5, reviewCount: 0 }; // Mặc định 5 sao nếu chưa có đánh giá
    }

    const total = data.reduce((sum, r) => sum + r.rating, 0);
    return {
      avgRating: total / data.length,
      reviewCount: data.length
    };
  } catch {
    return { avgRating: 5, reviewCount: 0 };
  }
};

/* =======================
   NAVIGATE DETAIL
======================= */
const goToDetail = (id) => {
  router.push(`/combo/${id}`);
};

/* =======================
   ADD TO CART
======================= */
const addToCart = async (comboId, isAvailable) => {
  if (!isAvailable) return;

  if (userRole.value === "Admin") {
    alert("Admin không thể mua hàng!");
    return;
  }

  if (!userToken.value) {
    redirectToLogin();
    return;
  }

  try {
    const { data: cart } = await api.get("/Cart", {
      headers: { Authorization: `Bearer ${userToken.value}` }
    });

    const existing = cart.items?.find(i => i.productId === comboId);
    const quantity = existing ? existing.quantity + 1 : 1;

    await api.post(
      "/Cart/product",
      { productId: comboId, quantity },
      { headers: { Authorization: `Bearer ${userToken.value}` } }
    );

    alert("Đã thêm combo vào giỏ hàng!");
  } catch (error) {
    console.error("Add to cart error:", error);
    alert("Không thể thêm vào giỏ hàng");
  }
};

/* =======================
   LOGIN REDIRECT
======================= */
const redirectToLogin = () => {
  if(confirm("Bạn cần đăng nhập để mua hàng. Đến trang đăng nhập ngay?")){
      router.push("/login");
  }
};

/* =======================
   IMAGE HANDLE
======================= */
const resolveImage = (imageUrl) => {
  if (!imageUrl) return "/img/default-combo.jpg"; // Ảnh mặc định
  return imageUrl.startsWith("http") ? imageUrl : `https://localhost:7119${imageUrl}`;
};

const onImgError = (e) => {
  e.target.src = "https://placehold.co/600x400?text=Combo+Image";
};

/* =======================
   FORMAT CURRENCY (VND)
======================= */
const formatPrice = (value) => {
    const number = Number(value);
    if (isNaN(number) || number === 0) return '0 VND';
    
    // Format dạng 100.000 + chữ VND
    return number.toLocaleString('vi-VN') + ' VND';
};

/* =======================
   LIFECYCLE
======================= */
onMounted(() => {
  fetchUserSession();
  fetchCombos();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;500;600;700&display=swap');

/* Định nghĩa biến màu giống trang Food */
:root {
    --primary: #ff3366;        /* Màu hồng chủ đạo */
    --primary-hover: #e6004c;
    --bg-page: #fff0f5;
    --font-main: 'Quicksand', sans-serif;
}

.combo-page {
    font-family: var(--font-main);
    background-color: var(--bg-page);
    min-height: 100vh;
}

.page-title {
    color: #ec5b98;
    font-weight: 800;
    margin-bottom: 5px;
}
.page-title i { color: var(--primary); }

/* --- CARD STYLE (Giống Food Card nhưng giữ kích thước cũ) --- */
.combo-card {
    border-radius: 20px;
    background: white;
    border: 1px solid rgba(255, 180, 200, 0.4); /* Viền hồng nhạt */
    box-shadow: 0 8px 20px rgba(255, 51, 102, 0.05);
    transition: all 0.35s ease;
    cursor: pointer;
    overflow: hidden;
    position: relative;
}

.combo-card:hover {
    transform: translateY(-8px);
    box-shadow: 0 15px 30px rgba(255, 51, 102, 0.2);
    border-color: var(--primary);
}

/* Ảnh giữ nguyên kích thước 180px như yêu cầu */
.img-wrapper {
    overflow: hidden;
    height: 180px; 
    border-bottom: 1px solid #fff0f5;
}

.card-img-top {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.5s ease;
}

.combo-card:hover .card-img-top {
    transform: scale(1.1); /* Zoom ảnh khi hover */
}

/* Nội dung card */
.card-body {
    padding: 18px;
}

.card-title {
    font-size: 1.2rem;
    font-weight: 700;
    color: #333;
    margin-bottom: 8px;
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
.combo-card:hover .card-title { color: var(--primary); }

.card-text {
    color: #666;
    font-size: 0.95rem;
    line-height: 1.5;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    margin-bottom: 15px;
}

/* --- BADGES --- */
.badge-wrapper { position: absolute; top: 12px; left: 12px; z-index: 2; }

.badge-sold-out {
    background-color: #333; color: white;
    padding: 5px 12px; border-radius: 8px;
    font-weight: 700; font-size: 0.75rem;
}

/* Xử lý khi hết hàng */
.unavailable .img-wrapper { filter: grayscale(100%); opacity: 0.7; }
.unavailable .card-body { opacity: 0.6; }

/* --- GIÁ VÀ NÚT BẤM --- */
.price-tag {
    color: #ff3366 !important;
    font-weight: 800;
    font-size: 1.25rem;
}

.btn-buy {
    background-color: #ffe6ee; /* Nền hồng nhạt */
    color: rgb(146, 89, 98);     /* Chữ hồng đậm */
    font-weight: 700;
    border: none;
    border-radius: 12px;
    padding: 10px 16px;
    transition: all 0.3s;
}

.btn-buy:hover:not(:disabled) {
    background-color: pink; /* Hover thành nền đỏ hồng */
    box-shadow: 0 4px 12px rgba(255, 51, 102, 0.4);
}

.btn-buy:disabled {
    background-color: #eee;
    color: #bbb;
    cursor: not-allowed;
}

/* Rating Stars */
.rating-stars i { font-size: 0.9rem; margin-right: 2px; }
</style>