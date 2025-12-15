<template>
  <div class="product-page-wrapper py-5">
    <div class="container">

      <!-- Loading -->
      <div v-if="!food" class="text-center py-5 loading-box">
        <div class="spinner-border text-pink" role="status">
          <span class="visually-hidden">Đang tải...</span>
        </div>
        <p class="mt-3 text-pink fw-bold">Đang lấy bánh ra lò...</p>
      </div>

      <!-- Main content -->
      <div class="main-card p-4 p-lg-5" v-if="food">
        <div class="row g-5">
          <!-- Hình ảnh sản phẩm -->
          <div class="col-lg-5">
            <div class="product-image-frame">
              <img 
                :src="resolveImage(food.imageUrl, food.categoryName)"
                class="img-fluid product-img"
                :alt="food.name"
                @error="onImgError"
              />
              <div class="sticker-decoration">✨</div>
            </div>
          </div>

          <!-- Thông tin sản phẩm -->
          <div class="col-lg-7 d-flex flex-column">
            <div class="product-info">
              <span class="badge-category mb-2">{{ food.categoryName || 'Menu đặc biệt' }}</span>
              <h1 class="product-name mb-3">{{ food.name }}</h1>

              <!-- Rating trung bình -->
              <div class="d-flex align-items-center mb-4 rating-mini">
                <div class="stars me-2">
                  <span v-for="n in 5" :key="n" :class="n <= Math.round(avgRating) ? 'text-warning' : 'text-muted-light'">
                    <i class="bi bi-star-fill"></i>
                  </span>
                </div>
                <span class="text-muted small">({{ reviews.length }} đánh giá)</span>
              </div>

              <p class="product-desc mb-4">
                {{ food.description || 'Hương vị tuyệt vời đang chờ bạn khám phá. Một sự kết hợp hoàn hảo của nguyên liệu tươi ngon.' }}
              </p>

              <div class="price-box mb-4">
                <span class="label">Giá bán:</span>
                <span class="price">{{ formatCurrency(food.price) }}</span>
              </div>

              <div class="action-buttons">
                 <button class="btn btn-outline-pink rounded-pill px-4" @click="$router.go(-1)">
                   <i class="bi bi-arrow-left me-2"></i> Quay lại menu
                 </button>
              </div>
            </div>
          </div>
        </div>

        <hr class="my-5 divider-pink">

        <!-- Review section -->
        <div class="review-section">
          <h3 class="section-title mb-4">
            <i class="bi bi-chat-heart-fill me-2 text-pink"></i> Đánh giá từ khách hàng
          </h3>

          <!-- Form review (chỉ user bình thường) -->
          <div class="review-form-box mb-5 shadow-sm" v-if="token && !isAdmin">
            <h5 class="form-title mb-3">Bạn cảm thấy món này thế nào?</h5>

            <div class="d-flex align-items-center mb-3 star-select-area">
              <span class="me-3 fw-bold text-secondary">Chấm điểm:</span>
              <div class="star-rating-input">
                <i v-for="n in 5" 
                   :key="n" 
                   class="bi" 
                   :class="n <= newReview.rating ? 'bi-star-fill text-warning' : 'bi-star text-muted-light'"
                   @click="newReview.rating = n">
                </i>
              </div>
              <span class="ms-3 badge bg-pink-light text-pink fw-bold" v-if="newReview.rating > 0">
                {{ newReview.rating }}/5 Tuyệt vời
              </span>
            </div>

            <textarea v-model="newReview.comment"
                      class="form-control custom-textarea mb-3"
                      rows="3"
                      placeholder="Hãy chia sẻ cảm nhận ngọt ngào của bạn..."></textarea>

            <div class="text-end">
                <button class="btn btn-pink-gradient px-4 py-2" @click="submitReview">
                    Gửi đánh giá <i class="bi bi-send-fill ms-1"></i>
                </button>
            </div>
          </div>

          <!-- Danh sách review -->
          <div v-if="reviews.length > 0" class="review-list">
            <div v-for="review in reviews" :key="review.id" class="review-item mb-3">
              <div class="d-flex">
                <div class="avatar-placeholder me-3">
                    {{ review.fullName ? review.fullName.charAt(0).toUpperCase() : 'U' }}
                </div>
                <div class="flex-grow-1">
                    <div class="d-flex justify-content-between align-items-center">
                        <h6 class="user-name mb-0">{{ review.fullName || 'Ẩn danh' }}</h6>
                        <small class="text-muted post-date">{{ new Date(review.createdAt).toLocaleDateString('vi-VN') }}</small>
                    </div>
                    <div class="user-rating mb-1">
                        <i v-for="n in 5" :key="n" 
                           class="bi bi-star-fill" 
                           :class="n <= review.rating ? 'text-warning' : 'text-gray-200'"></i>
                    </div>
                    <!-- Nội dung comment -->
                    <p class="user-comment mb-0" v-if="!isAdmin">{{ review.comment }}</p>
                    <p class="user-comment text-muted fst-italic mb-0" v-else>Nội dung ẩn (Admin view)</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Empty state -->
          <div v-else class="empty-state text-center py-4 rounded-3">
            <i class="bi bi-chat-square-heart text-pink-light display-4"></i>
            <p class="text-muted mt-2">Chưa có đánh giá nào. Hãy là người đầu tiên nếm thử nhé!</p>
          </div>

        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import api from "@/api";

const route = useRoute();

const food = ref(null);         
const reviews = ref([]);
const newReview = ref({ rating: 0, comment: "" });

const token = ref(null);
const isAdmin = ref(false);

/* ======================
   COMPUTED
====================== */
const isCombo = computed(() => route.path.startsWith("/combo"));
const productId = computed(() => food.value?.product?.id || food.value?.productId || food.value?.id || null);

const avgRating = computed(() => {
  if (!reviews.value.length) return 0;
  return reviews.value.reduce((sum, r) => sum + r.rating, 0) / reviews.value.length;
});

const formatCurrency = (amount) => amount ? Number(amount).toLocaleString("vi-VN") + " VND" : "0 VND";

/* ======================
   IMAGE
====================== */
const resolveImage = (imageUrl) => {
  if (!imageUrl) return "/food/default.jpg";
  return imageUrl.startsWith("http") ? imageUrl : `https://localhost:7119${imageUrl}`;
};

const onImgError = (e) => { e.target.src = "/food/default.jpg"; };

/* ======================
   FETCH DATA
====================== */
const fetchDetail = async () => {
  try {
    const id = route.params.id;
    const res = isCombo.value ? await api.get(`/Combo/${id}`) : await api.get(`/Food/${id}`);
    food.value = Array.isArray(res.data) ? res.data[0] : res.data;
  } catch (err) { console.error("Lỗi lấy chi tiết:", err); }
};

const fetchReviews = async () => {
  try {
    if (!productId.value) return;

    const { data } = await api.get(`/Reviews/product/${productId.value}`, {
      headers: token.value ? { Authorization: `Bearer ${token.value}` } : {}
    });

    // Chỉ lấy review chưa bị xóa
    reviews.value = data.filter(r => !r.deletedAt);
  } catch (err) {
    console.error("Lỗi lấy review:", err);
  }
};


/* ======================
   SUBMIT REVIEW
====================== */
const submitReview = async () => {
  if (!token.value) return alert("Bạn cần đăng nhập");
  if (isAdmin.value) return alert("Admin không được phép gửi đánh giá.");
  if (!productId.value) return alert("Không xác định được sản phẩm");
  if (!newReview.value.rating || !newReview.value.comment) return alert("Vui lòng chọn sao và nhập nội dung");

  const payload = {
    ProductId: productId.value,
    Rating: newReview.value.rating,
    Comment: newReview.value.comment
  };

  try {
    await api.post("/Reviews", payload, { headers: { Authorization: `Bearer ${token.value}` } });
    alert("Gửi đánh giá thành công");
    newReview.value = { rating: 0, comment: "" };
    fetchReviews();
  } catch (err) {
    console.error("Lỗi gửi review:", err.response?.data || err);
    alert("Gửi đánh giá thất bại");
  }
};

/* ======================
   MOUNTED
====================== */
onMounted(async () => {
  token.value = localStorage.getItem("token");
  isAdmin.value = localStorage.getItem("isAdmin") === "true";

  await fetchDetail();
  await fetchReviews();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;500;600;700;800&display=swap');

/* --- GENERAL SETUP --- */
.product-page-wrapper {
  background-color: #fff9fc; /* Nền hồng siêu nhạt */
  min-height: 100vh;
  font-family: 'Quicksand', sans-serif;
  color: #444;
}

/* Biến màu nội bộ */
.text-pink { color: #ff4f8e; }
.text-pink-light { color: #ff8fae; }
.bg-pink-light { background-color: #fff0f5; }
.text-warning { color: #ffc107; } /* Vàng sao */
.text-gray-200 { color: #e9ecef; }
.text-muted-light { color: #ced4da; }

/* --- MAIN CARD --- */
.main-card {
  background: #fff;
  border-radius: 30px;
  box-shadow: 0 10px 40px rgba(255, 79, 142, 0.08);
}

/* --- IMAGE FRAME --- */
.product-image-frame {
  position: relative;
  padding: 15px;
  background: #fff;
  border: 1px solid #ffe6f0;
  box-shadow: 0 15px 30px rgba(255, 79, 142, 0.15);
  border-radius: 25px;
  transform: rotate(-2deg); /* Nghiêng nhẹ nghệ thuật */
  transition: transform 0.3s;
}

.product-image-frame:hover {
  transform: rotate(0deg) scale(1.02);
}

.product-img {
  width: 100%;
  height: 400px;
  object-fit: cover;
  border-radius: 20px;
}

.sticker-decoration {
  position: absolute;
  top: -10px;
  right: -10px;
  font-size: 2rem;
  animation: float 3s ease-in-out infinite;
}

@keyframes float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }

/* --- PRODUCT INFO --- */
.badge-category {
  display: inline-block;
  background: #fff0f5;
  color: #ff4f8e;
  font-size: 0.85rem;
  font-weight: 700;
  padding: 5px 15px;
  border-radius: 20px;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.product-name {
  font-weight: 800;
  font-size: 2.5rem;
  color: #333;
}

.product-desc {
  font-size: 1.1rem;
  line-height: 1.7;
  color: #666;
}

.price-box .label {
  font-size: 1rem;
  color: #888;
  margin-right: 10px;
}

.price-box .price {
  font-size: 2rem;
  font-weight: 800;
  color: #ff4f8e;
}

.btn-outline-pink {
    border: 2px solid #ff4f8e;
    color: #ff4f8e;
    font-weight: 600;
    transition: all 0.3s;
}
.btn-outline-pink:hover {
    background: #ff4f8e;
    color: #fff;
}

/* --- DIVIDER --- */
.divider-pink {
  border-top: 2px dashed #ffcce0;
  opacity: 1;
  background: transparent;
}

/* --- REVIEWS --- */
.section-title {
  font-weight: 700;
  color: #333;
}

/* Form Review */
.review-form-box {
  background: #fffcfd;
  border: 1px solid #ffe6f0;
  border-radius: 20px;
  padding: 30px;
}

.form-title {
  font-weight: 700;
  color: #555;
}

.star-rating-input i {
  font-size: 1.8rem;
  cursor: pointer;
  transition: transform 0.2s;
  margin-right: 5px;
}

.star-rating-input i:hover {
  transform: scale(1.2);
}

.custom-textarea {
  border: 2px solid #ffe6f0;
  border-radius: 15px;
  padding: 15px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.custom-textarea:focus {
  border-color: #ff4f8e;
  box-shadow: none;
}

.btn-pink-gradient {
  background: linear-gradient(90deg, #ff4f8e 0%, #ff8fae 100%);
  color: white;
  font-weight: 700;
  border: none;
  border-radius: 50px;
  box-shadow: 0 5px 15px rgba(255, 79, 142, 0.3);
  transition: transform 0.2s;
}

.btn-pink-gradient:hover {
  transform: translateY(-2px);
  background: linear-gradient(90deg, #e0457b 0%, #ff4f8e 100%);
}

/* Review Item */
.review-item {
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 15px;
  padding: 20px;
  transition: 0.2s;
}

.review-item:hover {
  border-color: #ffe6f0;
  box-shadow: 0 5px 15px rgba(0,0,0,0.03);
}

.avatar-placeholder {
  width: 50px;
  height: 50px;
  background: #ffe6f0;
  color: #ff4f8e;
  font-weight: 800;
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.user-name {
  font-weight: 700;
  color: #333;
}

.user-comment {
  color: #555;
  line-height: 1.5;
}

.post-date {
  font-size: 0.8rem;
}

/* Empty State */
.empty-state {
  background: #fafafa;
  border: 2px dashed #eee;
}

/* Loading */
.loading-box .spinner-border {
  width: 3rem; 
  height: 3rem;
}
</style>
