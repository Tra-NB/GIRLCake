<template>
  <div class="order-page">
    <div class="decor-circle circle-1"></div>
    <div class="decor-circle circle-2"></div>

    <div class="container relative-z">
      <header class="page-header">
        <h1 class="header-title">Quản lý đơn hàng</h1>
        <p class="header-subtitle">Theo dõi và cập nhật trạng thái đơn hàng của bạn</p>
      </header>

      <div class="main-card">
        <div class="table-wrapper">
          <table class="order-table">
            <thead>
              <tr>
                <th>Mã đơn</th>
                <th>Khách hàng</th>
                <th>Địa chỉ nhận</th>
                <th>Tiền hàng</th>
                <th>Voucher</th>
                <th>Thành tiền</th>
                <th>Ngày tạo</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="order in filteredOrders" :key="order.id">
                <td class="col-id">#{{ order.id }}</td>
                <td class="col-user">{{ order.userName || "Khách vãng lai" }}</td>
                <td class="col-address" :title="order.shippingAddress">{{ order.shippingAddress }}</td>
                <!-- Tiền hàng trừ 15000 -->
                <td>{{ formatCurrency((order.subtotal || order.totalAmount) - 15000) }}</td>
                <td>
                  <span v-if="order.voucherCode" class="tag-voucher">{{ order.voucherCode }}</span>
                  <span v-else class="text-gray">-</span>
                </td>
                <td class="col-total">{{ formatCurrency(order.totalAmount) }}</td>
                <td>{{ formatDate(order.createdAt) }}</td>
                <td class="col-status">
                  <div :class="['badge-status', getStatusClass(order.status)]">
                    <span class="status-dot"></span>
                    {{ getStatusLabel(order.status) }}
                  </div>
                </td>
                <td>
                  <button class="btn-view-detail" @click="viewOrder(order)" title="Xem chi tiết">
                    <i class="bi bi-eye-fill"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <div v-if="filteredOrders.length === 0" class="empty-state">
            <i class="bi bi-inbox"></i>
            <p>Chưa có đơn hàng nào.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Chi Tiết Đơn Hàng -->
    <div v-if="showModal" class="modal-backdrop" @click.self="closeModal">
      <div class="modal-content animate-fade-in">

        <div class="modal-header">
          <div class="header-left">
            <span class="label-small">Chi tiết đơn hàng</span>
            <h2 class="order-number">#{{ currentOrder.id }}</h2>
          </div>
          <button class="btn-close-modal" @click="closeModal">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>

        <div class="modal-body">
          <div class="layout-grid">

            <!-- Thông tin giao hàng -->
            <div class="column-info">
              <h3 class="section-heading">Thông tin giao hàng</h3>

              <div class="info-row">
                <label>Người nhận</label>
                <div class="value-text">
                  <i class="bi bi-person-circle icon-pink"></i> {{ currentOrder.userName || "Khách" }}
                </div>
              </div>

              <div class="info-row">
                <label>Địa chỉ</label>
                <div class="value-text">
                  <i class="bi bi-geo-alt-fill icon-pink"></i> {{ currentOrder.shippingAddress }}
                </div>
              </div>

              <div class="separator-line"></div>

              <!-- Tiền hàng trừ 15000 -->
              <div class="price-row">
                <span>Tiền hàng</span>
                <strong>{{ formatCurrency((currentOrder.subtotal || currentOrder.totalAmount) - 15000) }}</strong>
              </div>
              <div class="price-row">
                <span>Voucher</span>
                <span class="text-pink">{{ currentOrder.voucherCode || "Không áp dụng" }}</span>
              </div>
              <div class="price-row">
                <span>Ngày tạo</span>
                <strong>{{ formatDate(currentOrder.createdAt) }}</strong>
              </div>

              <div class="total-payment-card">
                <span>Tổng thanh toán</span>
                <div class="big-price">{{ formatCurrency(currentOrder.totalAmount) }}</div>
              </div>

              <div class="status-update-area">
                <label>Cập nhật trạng thái</label>
                <div class="select-container">
                  <select
                    v-model="newStatus"
                    :disabled="!isAdmin || currentOrder.status === 3"
                    class="status-select"
                  >
                    <option v-for="opt in orderStatusOptions" :key="opt.value" :value="opt.value">
                      {{ opt.label }}
                    </option>
                  </select>
                  <i class="bi bi-chevron-down arrow-icon"></i>
                </div>
              </div>
            </div>

            <!-- Danh sách sản phẩm -->
            <div class="column-products">
              <h3 class="section-heading">Danh sách sản phẩm</h3>
              <div class="product-table-wrapper">
                <table class="product-list-table">
                  <thead>
                    <tr>
                      <th>Sản phẩm</th>
                      <th class="text-center">SL</th>
                      <th class="text-right">Đơn giá</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="item in currentOrder.orderDetails || []" :key="item.productId">
                      <td>
                        <div class="product-name">{{ item.productName }}</div>
                      </td>
                      <td class="text-center"><span class="qty-badge">x{{ item.quantity }}</span></td>
                      <td class="text-right">{{ formatCurrency(item.unitPrice) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-cancel" @click="closeModal">Đóng</button>
          <button
            v-if="isAdmin"
            class="btn-save"
            @click="updateOrder"
            :disabled="currentOrder.status === 3"
          >
            <i class="bi bi-check-circle-fill"></i> Cập nhật
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api';

const orders = ref([]);
const showModal = ref(false);
const currentOrder = ref({});
const newStatus = ref(null);

const userToken = ref(localStorage.getItem("token") || "");
const currentUser = ref(JSON.parse(localStorage.getItem("user") || "{}"));
const isAdmin = computed(() => currentUser.value.role?.trim().toLowerCase() === "admin");

const orderStatusOptions = [
  { label: 'Đang xử lý', value: 1 },
  { label: 'Đang giao', value: 2 },
  { label: 'Hoàn tất', value: 3 },
];

const filteredOrders = computed(() => {
  if (isAdmin.value) return orders.value;
  return orders.value.filter(o => String(o.userId) === String(currentUser.value.id));
});

const fetchOrders = async () => {
  try {
    const res = await api.get('/Order', { headers: { Authorization: `Bearer ${userToken.value}` } });
    orders.value = res.data.map(o => ({
      ...o,
      voucherCode: o.voucherCode || null
    }));
  } catch (error) {
    console.error('Lấy đơn hàng thất bại:', error);
  }
};

const fetchOrderDetails = async (orderId) => {
  try {
    const res = await api.get(`/Order/${orderId}`, { headers: { Authorization: `Bearer ${userToken.value}` } });
    currentOrder.value = { ...res.data, voucherCode: res.data.voucherCode || null };
    newStatus.value = currentOrder.value.status;
    showModal.value = true;
  } catch (error) {
    console.error('Lấy chi tiết đơn hàng thất bại:', error);
  }
};

const viewOrder = (order) => fetchOrderDetails(order.id);
const closeModal = () => { showModal.value = false; currentOrder.value = {}; newStatus.value = null; };

const updateOrder = async () => {
  if (!isAdmin.value || currentOrder.value.status === 3) return;
  try {
    await api.put(`/Order/${currentOrder.value.id}`, { status: newStatus.value }, { headers: { Authorization: `Bearer ${userToken.value}` } });
    await fetchOrders();
    closeModal();
  } catch (error) {
    console.error('Cập nhật đơn hàng thất bại:', error);
    alert('Cập nhật thất bại');
  }
};

// --- Format Helpers ---
const formatCurrency = (value) => {
  if (!value) return "0 VND";
  return value.toLocaleString("vi-VN") + " VND";
};

const formatDate = (date) => {
  if (!date) return "-";
  const d = new Date(date);
  return d.toLocaleDateString("vi-VN") + " " + d.toLocaleTimeString("vi-VN", { hour: '2-digit', minute: '2-digit' });
};

const getStatusClass = (status) => {
  switch(status) {
    case 1: return 'status-pending';
    case 2: return 'status-shipping';
    case 3: return 'status-completed';
    default: return '';
  }
};

const getStatusLabel = (status) => {
  const opt = orderStatusOptions.find(o => o.value === status);
  return opt ? opt.label : 'N/A';
};

onMounted(fetchOrders);
</script>

<style scoped>
.order-table th:nth-child(4),
.order-table td:nth-child(4) {
  min-width: 120px; /* cột tiền hàng */
}
</style>
<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@500;600;700&display=swap');

/* --- CẤU TRÚC CHUNG --- */
.order-page {
  min-height: 100vh;
  padding: 40px 20px;
  font-family: 'Quicksand', sans-serif;
  position: relative;
  overflow-x: hidden;
  color: #333;
}

.container { max-width: 1200px; margin: 0 auto; position: relative; z-index: 1; }

/* Header Trang */
.page-header { text-align: center; margin-bottom: 30px; }
.header-title { color: #d63384; font-size: 32px; font-weight: 800; margin-bottom: 5px; }
.header-subtitle { color: #999; font-size: 15px; }

/* Trang trí nền (Mờ ảo) */
.decor-circle {
  position: absolute; border-radius: 50%; filter: blur(80px); opacity: 0.4; z-index: 0;
}
/* --- CARD BẢNG ĐƠN HÀNG --- */
.main-card {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(12px);
  border-radius: 20px;
  border: 1px solid rgba(240, 240, 240, 0.8);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.05);
  padding: 10px;
  overflow: hidden;
}

.table-wrapper { overflow-x: auto; }
.order-table { width: 100%; border-collapse: separate; border-spacing: 0; }

.order-table th {
  background: transparent;
  color: #d63384;
  font-weight: 700;
  padding: 15px;
  font-size: 14px;
  text-transform: uppercase;
  border-bottom: 2px solid #f0f0f0;
  white-space: nowrap;
}

.order-table td {
  padding: 14px 15px;
  background: transparent;
  border-bottom: 1px solid #f8f8f8;
  vertical-align: middle;
  font-size: 14px;
}

.order-table tbody tr:hover td { background: #fffafa; }

/* Style cho các cột */
.col-id { font-weight: 700; color: #d63384; }
.col-user { font-weight: 700; color: #333; }
.col-address { max-width: 180px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: #666; }
.col-total { font-weight: 700; color: #d63384; }
.text-gray { color: #ccc; }

.tag-voucher {
  background: #fff; border: 1px solid #d63384; color: #d63384;
  padding: 3px 8px; border-radius: 6px; font-size: 11px; font-weight: 600;
}

/* Badge Trạng Thái */
.badge-status {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 5px 12px; border-radius: 20px;
  font-size: 12px; font-weight: 700;
}
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }
.status-pending { background: #fff8e1; color: #ffa000; } 
.status-shipping { background: #e3f2fd; color: #1976d2; } 
.status-completed { background: #e8f5e9; color: #388e3c; } 

/* Nút Xem */
.btn-view-detail {
  width: 32px; height: 32px;
  border-radius: 8px; border: 1px solid #ffebf0;
  background: white; color: #d63384;
  cursor: pointer; transition: 0.2s;
  display: flex; align-items: center; justify-content: center;
}
.btn-view-detail:hover { background: #d63384; color: white; border-color: #d63384; }

.empty-state { text-align: center; padding: 40px; color: #999; }
.empty-state i { font-size: 32px; display: block; margin-bottom: 10px; opacity: 0.5; }

/* --- MODAL (POPUP) STYLE --- */
.modal-backdrop {
  position: fixed; inset: 0;
  background: rgba(0, 0, 0, 0.4); /* Nền tối nhẹ */
  backdrop-filter: blur(4px);
  display: flex; justify-content: center; align-items: center; 
  z-index: 999;
}

.modal-content {
  background: white;
  border-radius: 16px;
  width: 100%; 
  max-width: 750px; /* Nhỏ lại theo yêu cầu */
  max-height: 85vh; /* Giới hạn chiều cao */
  display: flex; flex-direction: column;
  box-shadow: 0 20px 60px rgba(0,0,0,0.2);
  animation: slideUp 0.3s ease-out;
}

@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }

/* Modal Header */
.modal-header {
  padding: 15px 25px; border-bottom: 1px solid #f0f0f0;
  display: flex; justify-content: space-between; align-items: center;
  flex-shrink: 0;
}
.label-small { font-size: 11px; color: #999; text-transform: uppercase; letter-spacing: 0.5px; display: block; }
.order-number { font-size: 20px; color: #d63384; margin: 0; font-weight: 800; }
.btn-close-modal { background: none; border: none; font-size: 18px; color: #ccc; cursor: pointer; padding: 5px; }
.btn-close-modal:hover { color: #d63384; }

/* Modal Body - Scrollable */
.modal-body {
  padding: 25px;
  overflow-y: auto; /* Cho phép cuộn nội dung giữa */
  flex-grow: 1;
}

.layout-grid { display: grid; grid-template-columns: 4fr 6fr; gap: 25px; }

/* Cột Trái Modal */
.column-info { padding-right: 20px; border-right: 1px dashed #e0e0e0; width: 336px; }
.section-heading { font-size: 15px; font-weight: 700; color: #333; margin-bottom: 15px; }

.info-row { margin-bottom: 12px; }
.info-row label { display: block; font-size: 11px; color: #aaa; margin-bottom: 2px; }
.value-text { font-size: 14px; font-weight: 600; color: #333; display: flex; align-items: center; gap: 6px; }
.icon-pink { color: #d63384; font-size: 14px; }

.separator-line { height: 1px; background: #f0f0f0; margin: 15px 0; }

.price-row { display: flex; justify-content: space-between; font-size: 13px; margin-bottom: 6px; color: #555; }
.text-pink { color: #d63384; font-weight: 600; }

.total-payment-card {
  background: linear-gradient(135deg, #d63384, #f06292);
  color: white; padding: 12px 15px; border-radius: 10px;
  display: flex; justify-content: space-between; align-items: center;
  margin-top: 15px; box-shadow: 0 4px 12px rgba(214, 51, 132, 0.2);
}
.big-price { font-size: 18px; font-weight: 700; }

/* Select Box */
.status-update-area { margin-top: 20px; }
.select-container { position: relative; }
.status-select {
  width: 100%; padding: 10px; border-radius: 8px; border: 1px solid #e0e0e0;
  appearance: none; background: #fff; font-weight: 600; color: #333; cursor: pointer;
  font-size: 13px;
}
.status-select:focus { border-color: #d63384; outline: none; }
.arrow-icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); pointer-events: none; font-size: 10px; color: #666; }

/* Cột Phải Modal */
.column-products { padding-left: 5px; }
.product-table-wrapper { border: 1px solid #f0f0f0; border-radius: 10px; overflow: hidden; }
.product-list-table { width: 100%; border-collapse: collapse; }
.product-list-table th { 
  font-size: 11px; color: #999; text-transform: uppercase; 
  padding: 10px 12px; background: #fafafa; border-bottom: 1px solid #f0f0f0; 
  text-align: left;
}
.product-list-table td { padding: 10px 12px; font-size: 13px; border-bottom: 1px solid #f0f0f0; color: #333; }
.product-list-table tr:last-child td { border-bottom: none; }
.product-name { font-weight: 600; }
.qty-badge { background: #f5f5f5; padding: 2px 6px; border-radius: 4px; font-size: 11px; font-weight: 700; color: #666; }
.text-center { text-align: center; }
.text-right { text-align: right; }

/* Modal Footer */
.modal-footer {
  padding: 15px 25px; background: #fcfcfc; border-top: 1px solid #f0f0f0;
  display: flex; justify-content: flex-end; gap: 10px;
  flex-shrink: 0;
  border-bottom-left-radius: 16px; border-bottom-right-radius: 16px;
}
.btn-cancel { background: #f5f5f5; border: none; padding: 8px 18px; border-radius: 8px; font-weight: 600; color: #666; cursor: pointer; }
.btn-cancel:hover { background: #eee; }
.btn-save {
  background: linear-gradient(135deg, #d63384, #ff4081); color: white; border: none; padding: 8px 20px; border-radius: 8px; font-weight: 600; cursor: pointer;
  display: flex; align-items: center; gap: 6px; transition: 0.2s;
}
.btn-save:hover { transform: translateY(-1px); box-shadow: 0 4px 10px rgba(214, 51, 132, 0.3); }
.btn-save:disabled { background: #ccc; cursor: not-allowed; transform: none; box-shadow: none; }

/* Responsive Mobile */
@media (max-width: 768px) {
  .modal-content { width: 95%; max-height: 90vh; }
  .layout-grid { grid-template-columns: 1fr; gap: 20px; }
  .column-info { border-right: none; border-bottom: 1px dashed #e0e0e0; padding-right: 0; padding-bottom: 20px; }
  .column-products { padding-left: 0; }
  .modal-body { padding: 20px; }
}
</style>