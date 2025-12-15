<template>
  <div class="page-wrapper">
    <div class="bg-decoration-1"></div>
    <div class="bg-decoration-2"></div>

    <div class="container-wide">
      <!-- HEADER -->
      <header class="glass-header">
        <div class="header-left">
          <h1><i class="bi bi-grid-fill"></i> Trung tâm phản hồi</h1>
          <p>Tổng hợp phản hồi</p>
        </div>
        <div class="header-right">
          <div class="stat-badge">
            <span class="stat-num">{{ visibleReviews.length }}</span>
            <span class="stat-label">Tổng đánh giá</span>
          </div>
        </div>
      </header>

      <!-- EMPTY STATE -->
      <div v-if="visibleReviews.length === 0" class="empty-state">
        <div class="empty-icon"><i class="bi bi-inbox"></i></div>
        <h3>Chưa có dữ liệu</h3>
        <p>Hiện tại chưa có đánh giá nào được gửi đến.</p>
      </div>

      <!-- REVIEW GRID -->
      <div v-else class="review-grid">
        <div class="chat-card" v-for="(r, index) in visibleReviews" :key="r.id" :style="{ animationDelay: index * 0.05 + 's' }">
          
          <!-- CARD TOP -->
          <div class="card-top">
            <div class="user-meta">
              <div class="avatar-ring">
                <div class="avatar">{{ getInitials(r.fullName) }}</div>
              </div>
              <div class="meta-info">
                <span class="name">{{ r.fullName }}</span>
                <span class="product">Mua: <strong>{{ r.productName }}</strong></span>
              </div>
            </div>
            <div class="rating-badge">
              {{ r.rating }} <i class="bi bi-star-fill"></i>
            </div>
          </div>

          <!-- CARD BODY -->
          <div class="card-body">
            <div class="msg-bubble user-msg">
              <p>{{ r.comment }}</p>
              <span class="time">{{ formatDate(r.createdAt) }}</span>
            </div>

            <div class="connector-line" v-if="r.adminReply"></div>

            <div class="msg-bubble admin-msg" v-if="r.adminReply">
              <div class="admin-icon"><i class="bi bi-shop"></i></div>
              <div class="bubble-content">
                <p>{{ r.adminReply }}</p>
                <span class="status-check">Đã trả lời <i class="bi bi-check2-all"></i></span>
              </div>
            </div>
          </div>

          <!-- CARD ACTIONS (ADMIN) -->
          <div class="card-actions" v-if="isAdmin">
            <div class="action-status" v-if="!r.adminReply && !r.deletedAt">
              <span class="pending-dot"></span> Chờ phản hồi
            </div>
            <div class="action-buttons">
              <!-- Nút Ẩn/Hiện -->
              <button 
                class="btn-icon" 
                @click="toggleVisibility(r)" 
                :title="r.deletedAt ? 'Hiện' : 'Ẩn'"
              >
                <i :class="r.deletedAt ? 'bi bi-eye' : 'bi bi-eye-slash'"></i>
              </button>

              <!-- Nút Trả lời/Sửa -->
              <button class="btn-reply" @click="viewReview(r)">
                {{ r.adminReply ? 'Sửa' : 'Trả lời ngay' }} <i class="bi bi-arrow-right"></i>
              </button>
            </div>
          </div>

        </div>
      </div>
    </div>

    <!-- MODAL TRẢ LỜI -->
    <transition name="pop">
      <div v-if="showModal && isAdmin" class="modal-overlay" @click.self="closeModal">
        <div class="glass-modal">
          <div class="modal-head">
            <h3>Soạn phản hồi</h3>
            <button class="close-icon" @click="closeModal"><i class="bi bi-x-lg"></i></button>
          </div>

          <div class="modal-content-area">
            <div class="review-quote">
              <small>Khách hàng viết:</small>
              <p>"{{ currentReview.comment }}"</p>
            </div>
            
            <div class="input-wrapper">
              <textarea 
                v-model="currentReview.adminReply" 
                placeholder="Nhập nội dung..." 
                rows="5"
              ></textarea>
            </div>
          </div>

          <div class="modal-foot">
            <button class="btn-cancel" @click="closeModal">Đóng</button>
            <button class="btn-gradient" @click="saveReply">
              Gửi phản hồi <i class="bi bi-send-fill"></i>
            </button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import api from "@/api";

// --- CONFIG ---
const userToken = ref(localStorage.getItem("token") || "");
const currentUser = ref(JSON.parse(localStorage.getItem("user") || "{}"));
const isAdmin = computed(() => currentUser.value.role?.trim().toLowerCase() === "admin");

// --- DATA ---
const reviews = ref([]);
const showModal = ref(false);
const currentReview = ref({});
const DEFAULT_REPLY = "Cảm ơn bạn đã tin tưởng và ủng hộ GIRLCake ạ! ❤️ Mong bạn sẽ tiếp tục đồng hành cùng GIRLCake trong thời gian tới nhé!";

// --- API ---
const fetchReviews = async () => {
  try {
    const res = await api.get("/Reviews", { headers: { Authorization: `Bearer ${userToken.value}` } });
    reviews.value = isAdmin.value ? res.data : res.data.filter(r => r.userId === currentUser.value.id);
  } catch (err) {
    console.error("Lỗi:", err);
  }
};

// --- LOGIC ---
const viewReview = (review) => {
  if (!isAdmin.value) return;
  currentReview.value = { ...review };
  if (!currentReview.value.adminReply) currentReview.value.adminReply = DEFAULT_REPLY;
  showModal.value = true;
};

const closeModal = () => showModal.value = false;

const saveReply = async () => {
  try {
    await api.put(`/Reviews/${currentReview.value.id}`, { AdminReply: currentReview.value.adminReply }, { headers: { Authorization: `Bearer ${userToken.value}` } });
    await fetchReviews();
    closeModal();
  } catch (err) { alert("Lỗi lưu"); }
};

const toggleVisibility = async (review) => {
  const action = review.deletedAt ? "hiện" : "ẩn";
  if (!confirm(`Bạn có muốn ${action} đánh giá này không?`)) return;

  try {
    await api.put(`/Reviews/${review.id}/toggle-visibility`, {}, { headers: { Authorization: `Bearer ${userToken.value}` } });
    await fetchReviews();
  } catch (err) {
    alert("Lỗi khi thay đổi trạng thái ẩn/hiện");
    console.error(err);
  }
};

const visibleReviews = computed(() => reviews.value);
const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN", {day: '2-digit', month: '2-digit'}) : "";
const getInitials = (name) => name ? name.split(' ').map(w => w[0]).join('').slice(0,2).toUpperCase() : 'U';

onMounted(fetchReviews);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&display=swap');

/* GLOBAL */
.page-wrapper {
  min-height: 100vh;
  background-color: #fff0f5; /* Hồng rất nhạt */
  font-family: 'Plus Jakarta Sans', sans-serif;
  color: #334155;
  padding: 30px;
  position: relative;
}

/* Background Decoration */
.bg-decoration-1 { position: absolute; top: -50px; left: -50px; width: 400px; height: 400px; background: #fbcfe8; filter: blur(90px); opacity: 0.6; z-index: 0; border-radius: 50%; }
.bg-decoration-2 { position: absolute; bottom: 0; right: -50px; width: 500px; height: 500px; background: #fce7f3; filter: blur(100px); opacity: 0.5; z-index: 0; border-radius: 50%; }

/* Container rộng hơn để chứa 2 cột */
.container-wide { 
  max-width: 1200px; /* Tăng độ rộng */
  margin: 0 auto; 
  position: relative; 
  z-index: 1; 
}

/* HEADER */
.glass-header {
  background: rgba(255, 255, 255, 0.6); backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.8); border-radius: 20px;
  padding: 15px 25px; display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 25px; box-shadow: 0 4px 15px rgba(236, 72, 153, 0.05);
}
.header-left h1 { margin: 0; font-size: 22px; color: #db2777; font-weight: 800; display: flex; gap: 8px; align-items: center; }
.header-left p { margin: 2px 0 0 0; font-size: 13px; color: #64748b; }
.stat-badge { background: white; padding: 6px 14px; border-radius: 12px; font-weight: 700; color: #db2777; box-shadow: 0 2px 5px rgba(0,0,0,0.05); font-size: 14px; }
.stat-label { font-size: 11px; color: #94a3b8; font-weight: 600; text-transform: uppercase; margin-left: 5px; }

/* --- GRID LAYOUT 2 CỘT (QUAN TRỌNG) --- */
.review-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr); /* Chia 2 cột đều nhau */
  gap: 20px; /* Khoảng cách giữa các ô */
  padding-bottom: 40px;
}

/* Responsive: Về 1 cột khi màn hình nhỏ */
@media (max-width: 768px) {
  .review-grid { grid-template-columns: 1fr; }
}

/* CHAT CARD STYLE */
.chat-card {
  background: white; border-radius: 20px; padding: 20px;
  box-shadow: 0 8px 25px -5px rgba(236, 72, 153, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.8);
  display: flex; flex-direction: column;
  animation: fadeUp 0.5s ease forwards; opacity: 0; transform: translateY(15px);
  height: 100%; /* Để các thẻ trong cùng hàng cao bằng nhau */
}

.chat-card:hover { transform: translateY(-3px); box-shadow: 0 12px 30px -8px rgba(236, 72, 153, 0.2); transition: 0.3s; }

/* Header Card */
.card-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; padding-bottom: 12px; border-bottom: 1px dashed #fce7f3; }
.user-meta { display: flex; gap: 10px; align-items: center; }
.avatar-ring { border: 1px solid #fbcfe8; padding: 2px; border-radius: 50%; }
.avatar { width: 36px; height: 36px; background: linear-gradient(to bottom right, #fce7f3, #fbcfe8); color: #db2777; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 14px; }
.meta-info { display: flex; flex-direction: column; }
.meta-info .name { font-weight: 700; font-size: 14px; color: #1e293b; }
.meta-info .product { font-size: 11px; color: #64748b; }
.rating-badge { background: #fff1f2; color: #e11d48; font-size: 12px; font-weight: 700; padding: 4px 10px; border-radius: 20px; }

/* Body Card */
.card-body { flex: 1; display: flex; flex-direction: column; }
.connector-line { width: 2px; height: 10px; background: #e2e8f0; margin-left: 18px; margin-bottom: 4px; }

.msg-bubble { padding: 10px 14px; border-radius: 14px; font-size: 13px; line-height: 1.5; position: relative; max-width: 95%; }
.user-msg { background: #f8fafc; border: 1px solid #f1f5f9; border-top-left-radius: 4px; color: #334155; }
.user-msg .time { display: block; font-size: 10px; color: #94a3b8; margin-top: 4px; font-weight: 500; }

.admin-msg { margin-left: auto; background: linear-gradient(135deg, #ec4899, #db2777); color: white; border-bottom-right-radius: 4px; display: flex; gap: 8px; margin-top: 4px; box-shadow: 0 4px 10px rgba(236, 72, 153, 0.25); }
.admin-icon { width: 20px; height: 20px; background: rgba(255,255,255,0.2); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 11px; flex-shrink: 0; margin-top: 2px; }
.status-check { display: block; font-size: 10px; text-align: right; margin-top: 3px; opacity: 0.9; }

/* Actions Footer */
.card-actions { margin-top: 15px; padding-top: 12px; border-top: 1px solid #f8fafc; display: flex; justify-content: space-between; align-items: center; }
.pending-dot { display: inline-block; width: 8px; height: 8px; background: #f59e0b; border-radius: 50%; margin-right: 4px; }
.action-status { font-size: 11px; color: #f59e0b; font-weight: 600; display: flex; align-items: center; }

.action-buttons { display: flex; gap: 8px; margin-left: auto; }
.btn-icon { border: none; background: transparent; color: #94a3b8; cursor: pointer; padding: 6px; font-size: 14px; transition: 0.2s; border-radius: 8px; }
.btn-icon:hover { background: #fef2f2; color: #ef4444; }
.btn-reply { border: none; background: #fff1f2; color: #db2777; padding: 6px 14px; border-radius: 10px; font-size: 12px; font-weight: 700; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 4px; }
.btn-reply:hover { background: #fce7f3; transform: translateX(2px); }

/* MODAL */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.3); backdrop-filter: blur(4px); z-index: 999; display: flex; justify-content: center; align-items: center; }
.glass-modal { width: 500px; background: white; border-radius: 20px; overflow: hidden; box-shadow: 0 20px 60px rgba(0,0,0,0.2); animation: zoomIn 0.3s; }
.modal-head { padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #f1f5f9; }
.modal-head h3 { margin: 0; font-size: 16px; color: #334155; }
.close-icon { border: none; background: #f1f5f9; width: 28px; height: 28px; border-radius: 50%; cursor: pointer; color: #64748b; }

.modal-content-area { padding: 20px; background: #fafafa; }
.review-quote { background: white; padding: 12px; border-radius: 12px; border: 1px dashed #cbd5e1; margin-bottom: 15px; font-size: 13px; color: #475569; }
.review-quote small { color: #94a3b8; font-weight: 600; display: block; margin-bottom: 4px; }
.input-wrapper textarea { width: 100%; border: 1px solid #e2e8f0; border-radius: 12px; padding: 12px; font-size: 14px; font-family: inherit; resize: none; box-sizing: border-box; outline: none; transition: 0.2s; }
.input-wrapper textarea:focus { border-color: #db2777; background: white; }

.modal-foot { padding: 12px 20px; background: white; display: flex; justify-content: flex-end; gap: 10px; }
.btn-cancel { border: 1px solid #e2e8f0; background: white; padding: 8px 16px; border-radius: 8px; cursor: pointer; font-weight: 600; color: #64748b; }
.btn-gradient { border: none; background: linear-gradient(135deg, #ec4899, #db2777); color: white; padding: 8px 20px; border-radius: 8px; cursor: pointer; font-weight: 600; display: flex; align-items: center; gap: 6px; box-shadow: 0 4px 10px rgba(236, 72, 153, 0.3); }

@keyframes fadeUp { to { opacity: 1; transform: translateY(0); } }
@keyframes zoomIn { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>