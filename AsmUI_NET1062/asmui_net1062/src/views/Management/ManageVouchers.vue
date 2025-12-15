<template>
  <div class="dashboard-container">
    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">
          <i class="bi bi-ticket-perforated-fill"></i> Quản lý voucher
        </h1>
        <p class="sub-title">Danh sách mã giảm giá & khuyến mãi</p>
      </div>
      
      <div class="header-actions">
        <div class="search-box">
          <i class="bi bi-search"></i>
          <input v-model="searchTerm" type="text" placeholder="Tìm code..." />
        </div>
        <button class="btn-primary" @click="openModal()">
          <i class="bi bi-plus-lg"></i> Thêm Mới
        </button>
      </div>
    </header>

    <div class="content-card">
      <div class="table-responsive">
        <table class="modern-table">
          <thead>
            <tr>
              <th>Mã Code</th>
              <th>Mô tả</th>
              <th>Giá trị</th>
              <th>Đã dùng</th>
              <th>Thời gian</th>
              <th>Trạng thái</th>
              <th class="text-right">#</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="v in filteredVouchers" :key="v.id">
              <td>
                <span class="code-badge">{{ v.code }}</span>
              </td>
              <td class="text-muted">{{ v.description }}</td>
              <td>
                <div class="discount-tag" :class="v.discountType === 0 ? 'type-percent' : 'type-fixed'">
                  {{ v.discountType === 0 ? v.discountValue + '%' : formatCurrency(v.discountValue) + ' VND' }}
                </div>
              </td>
              <td>
                <span class="usage-text">{{ v.usedCount }}/{{ v.usageLimit || '∞' }}</span>
              </td>
              <td>
                <div class="date-range">
                  <span>{{ formatDate(v.startDate) }}</span>
                  <i class="bi bi-arrow-right-short"></i>
                  <span>{{ formatDate(v.endDate) }}</span>
                </div>
              </td>
              <td>
                <span :class="['status-badge', v.isActive ? 'status-active' : 'status-inactive']">
                  {{ v.isActive ? 'Active' : 'Stop' }}
                </span>
              </td>
              <td class="text-right">
                <button class="action-btn btn-edit" @click="editVoucher(v)">
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button class="action-btn btn-delete" @click="deleteVoucher(v.id)">
                  <i class="bi bi-trash"></i>
                </button>
              </td>
            </tr>
            <tr v-if="filteredVouchers.length === 0">
              <td colspan="7" class="empty-state">
                <i class="bi bi-inbox"></i> Không có dữ liệu
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <transition name="fade">
      <div v-if="showModal" class="modal-backdrop" @click.self="closeModal">
        <div class="modal-panel">
          <div class="modal-header">
            <h3>{{ modalTitle }}</h3>
            <button class="btn-close" @click="closeModal"><i class="bi bi-x-lg"></i></button>
          </div>
          
          <div class="modal-body">
            <div class="form-grid">
              <div class="form-group">
                <label>Mã Code</label>
                <input v-model="currentVoucher.code" placeholder="VD: SALE50" class="uppercase-input"/>
              </div>
              <div class="form-group">
                <label>Loại giảm</label>
                <select v-model="currentVoucher.discountType">
                  <option :value="0">Phần trăm (%)</option>
                  <option :value="1">Tiền mặt (VND)</option>
                </select>
              </div>

              <div class="form-group span-2">
                <label>Mô tả</label>
                <input v-model="currentVoucher.description" placeholder="Mô tả ngắn..." />
              </div>

              <div class="form-group">
                <label>Giá trị giảm</label>
                <div class="input-with-suffix">
                  <input type="number" v-model="currentVoucher.discountValue" />
                  <span class="suffix-text" v-if="currentVoucher.discountType === 1">
                    {{ formatCurrency(currentVoucher.discountValue) }} VND
                  </span>
                  <span class="suffix-text" v-else>%</span>
                </div>
              </div>
              <div class="form-group">
                <label>Giới hạn (0 = ∞)</label>
                <input type="number" v-model="currentVoucher.usageLimit" />
              </div>

              <div class="form-group">
                <label>Bắt đầu</label>
                <input type="date" v-model="currentVoucher.startDate" />
              </div>
              <div class="form-group">
                <label>Kết thúc</label>
                <input type="date" v-model="currentVoucher.endDate" />
              </div>

              <div class="form-group span-2 row-center">
                <label class="switch-container">
                  <span class="label-text">Kích hoạt voucher?</span>
                  <div class="toggle-switch">
                    <input type="checkbox" id="statusSwitch" v-model="currentVoucher.isActive">
                    <label for="statusSwitch" class="switch-label">
                      <span class="switch-inner"></span>
                      <span class="switch-switch"></span>
                    </label>
                  </div>
                </label>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-text" @click="closeModal">Hủy</button>
            <button class="btn-primary" @click="saveVoucher">Lưu Voucher</button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import api from '@/api';

const vouchers = ref([]);
const searchTerm = ref("");
const showModal = ref(false);
const currentVoucher = ref({});
const modalTitle = ref("");

// Format tiền VND
const formatCurrency = (value) => {
  if (!value && value !== 0) return '';
  return new Intl.NumberFormat('vi-VN').format(value);
};

// Format ngày
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('vi-VN');
};

const filteredVouchers = computed(() => {
  if (!searchTerm.value) return vouchers.value;
  const lowerTerm = searchTerm.value.toLowerCase();
  return vouchers.value.filter(v => 
    v.code.toLowerCase().includes(lowerTerm) || 
    v.description.toLowerCase().includes(lowerTerm)
  );
});

const fetchData = async () => {
  try {
    const res = await api.get('/Voucher');
    vouchers.value = res.data.map(v => {
      const startDate = v.startDate.split('T')[0];
      const endDate = v.endDate.split('T')[0];
      const today = new Date();
      today.setHours(0,0,0,0);
      const isExpired = new Date(endDate) < today;
      const isUsedUp = v.usageLimit > 0 && v.usedCount >= v.usageLimit;
      const isActive = (v.isActive === 1 || v.isActive === true) && !isExpired && !isUsedUp;

      return { ...v, startDate, endDate, isActive };
    });
  } catch (error) {
    console.error("Lỗi:", error);
  }
};

const openModal = () => {
  modalTitle.value = "Thêm Voucher";
  currentVoucher.value = {
    code: "",
    description: "",
    discountType: 0, 
    discountValue: 0,
    usageLimit: 0,
    startDate: new Date().toISOString().substr(0, 10),
    endDate: new Date().toISOString().substr(0, 10),
    isActive: true
  };
  showModal.value = true;
};

const editVoucher = (voucher) => {
  modalTitle.value = "Sửa Voucher";
  currentVoucher.value = { ...voucher };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveVoucher = async () => {
  try {
    const payload = { ...currentVoucher.value };
    if (modalTitle.value === "Thêm Voucher") {
      await api.post('/Voucher', payload);
    } else {
      await api.put(`/Voucher/${payload.id}`, payload);
    }
    await fetchData();
    closeModal();
  } catch (error) {
    alert("Lỗi khi lưu dữ liệu!");
  }
};

const deleteVoucher = async (id) => {
  if (!confirm("Xóa voucher này?")) return;
  try {
    await api.delete(`/Voucher/${id}`);
    await fetchData();
  } catch (error) {
    alert("Không thể xóa!");
  }
};

onMounted(fetchData);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@500;600;700&display=swap');

/* --- MAIN LAYOUT --- */
.dashboard-container {
  padding: 20px;
  background-color: #fff5f7;
  min-height: 100vh;
  font-family: 'Quicksand', sans-serif;
  color: #444;
}

.page-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 20px; flex-wrap: wrap; gap: 15px;
}
.page-title { font-size: 22px; color: #d63384; font-weight: 700; margin: 0; }
.page-title i { margin-right: 8px; }
.sub-title { font-size: 13px; color: #888; margin: 4px 0 0 0; }

.header-actions { display: flex; gap: 10px; }
.search-box { position: relative; }
.search-box input {
  padding: 8px 8px 8px 32px; border: 1px solid #f8bbd0;
  border-radius: 8px; outline: none; color: #d63384;
}
.search-box i { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: #d63384; }

.btn-primary {
  background: linear-gradient(135deg, #d63384, #ff4081);
  color: white; border: none; padding: 8px 16px; border-radius: 8px;
  font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 6px;
  box-shadow: 0 4px 8px rgba(214, 51, 132, 0.2);
}
.btn-primary:hover { transform: translateY(-1px); box-shadow: 0 6px 12px rgba(214, 51, 132, 0.3); }

/* --- TABLE --- */
.content-card {
  background: white; border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  border: 1px solid #ffeef4; overflow: hidden;
}
.modern-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.modern-table th {
  background: #fff0f6; color: #d63384; padding: 12px 16px;
  text-align: left; font-weight: 700; text-transform: uppercase; font-size: 12px;
}
.modern-table td { padding: 12px 16px; border-bottom: 1px solid #fff5f7; vertical-align: middle; }
.modern-table tr:hover { background: #fffbfd; }

.code-badge {
  background: #fce4ec; color: #c2185b; padding: 4px 8px;
  border-radius: 6px; font-weight: bold; font-family: monospace; border: 1px dashed #f48fb1;
}
.discount-tag {
  font-weight: 700; font-size: 13px; padding: 2px 8px; border-radius: 12px; display: inline-block;
}
.type-percent { color: #e91e63; background: #fce4ec; }
.type-fixed { color: #00897b; background: #e0f2f1; }

.usage-text { font-size: 13px; color: #666; }
.date-range { display: flex; align-items: center; gap: 4px; font-size: 12px; color: #888; }
.status-badge { font-size: 11px; padding: 3px 8px; border-radius: 10px; font-weight: 600; }
.status-active { background: #e8f5e9; color: #2e7d32; }
.status-inactive { background: #ffebee; color: #c62828; }

.action-btn { background: none; border: none; cursor: pointer; color: #bbb; padding: 4px; }
.btn-edit:hover { color: #d63384; } .btn-delete:hover { color: #f44336; }
.text-right { text-align: right; }
.empty-state { text-align: center; color: #ccc; padding: 20px; }

/* --- MODAL (Compact & Z-index fixed) --- */
.modal-backdrop {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.5); backdrop-filter: blur(2px);
  z-index: 9999;
  display: flex; justify-content: center; align-items: center;
}

.modal-panel {
  background: white;
  width: 500px;
  max-width: 90%;
  max-height: 90vh;
  border-radius: 16px;
  display: flex; flex-direction: column;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }

.modal-header {
  padding: 12px 20px; background: #fff0f6;
  display: flex; justify-content: space-between; align-items: center;
  border-bottom: 1px solid #fce4ec; border-radius: 16px 16px 0 0;
}
.modal-header h3 { margin: 0; font-size: 16px; color: #c2185b; font-weight: 700; }
.btn-close { background: none; border: none; font-size: 18px; cursor: pointer; color: #f48fb1; }

.modal-body { padding: 16px 20px; overflow-y: auto; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.span-2 { grid-column: span 2; }

.form-group label {
  display: block; font-size: 12px; font-weight: 600; color: #555; margin-bottom: 4px;
}
.form-group input, .form-group select {
  width: 100%; padding: 8px 10px;
  border: 1px solid #eee; border-radius: 6px;
  font-size: 13px; box-sizing: border-box;
}
.form-group input:focus, .form-group select:focus {
  border-color: #ec407a; outline: none; background: #fffbfd;
}
.uppercase-input { text-transform: uppercase; font-weight: bold; color: #d81b60; }

/* Input with Suffix (Preview VND) */
.input-with-suffix { position: relative; }
.input-with-suffix input {
  padding-right: 60px; /* Thêm padding phải để không đè chữ VND */
}
.suffix-text {
  position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
  font-size: 11px; color: #ec407a; font-weight: bold;
  background: #fce4ec; padding: 2px 6px; border-radius: 4px;
  pointer-events: none;
}

/* Toggle Switch Row */
.row-center { display: flex; justify-content: center; margin-top: 5px; }
.switch-container { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.toggle-switch { position: relative; width: 36px; height: 20px; }
.toggle-switch input { display: none; }
.switch-label {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  background-color: #ddd; border-radius: 20px; transition: .3s;
}
.switch-switch {
  position: absolute; height: 16px; width: 16px; left: 2px; bottom: 2px;
  background-color: white; border-radius: 50%; transition: .3s;
}
input:checked + .switch-label { background-color: #d81b60; }
input:checked + .switch-label .switch-switch { transform: translateX(16px); }

.modal-footer {
  padding: 12px 20px; border-top: 1px solid #fce4ec;
  display: flex; justify-content: flex-end; gap: 8px;
}
.btn-text { background: none; border: none; color: #777; font-size: 13px; cursor: pointer; font-weight: 600; }
.btn-text:hover { color: #333; }
</style>