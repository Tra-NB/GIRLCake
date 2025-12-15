<template>
  <div class="page-wrapper">
    <div class="content-container">

      <!-- Header -->
      <div class="header">
        <h1>Quản lý combo</h1>
        <button class="btn-add" @click="openModal()">+ Thêm Combo</button>
      </div>

      <!-- Grid Combo -->
      <div class="grid-container">
        <div v-for="combo in combos" :key="combo.id" class="food-card">
          <img 
            :src="getComboImage(combo)" 
            class="food-img" 
            alt="Combo Image"
            @error="onImgError"
          />
          <div class="info">
            <h2 class="name">{{ combo.name }}</h2>
            <p class="price">{{ combo.price.toLocaleString('vi-VN') }} VND</p>
            <p class="description-text" :title="combo.description">
              {{ combo.description || 'Chưa có mô tả' }}
            </p>
            <p class="status" :style="{ color: combo.isAvailable ? 'green' : 'red', fontWeight: 600 }">
              {{ combo.isAvailable ? 'Còn hàng' : 'Hết hàng' }}
            </p>
            <p class="items">
              <strong>Số món:</strong> {{ combo.items?.length || 0 }}
            </p>
            <div class="action-buttons">
              <button class="btn-edit" @click="openModal(combo.id)">Sửa</button>
              <button v-if="combo.deletedAt" class="btn-restore" @click="restoreCombo(combo.id)">Khôi phục</button>
              <button v-else class="btn-delete" @click="deleteCombo(combo.id)">Xóa</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Thêm/Sửa Combo -->
      <div v-if="showModal" class="modal-overlay">
        <div class="modal-box">

          <h2 class="modal-title">
            {{ editCombo ? "Sửa Combo" : "Thêm Combo" }}
          </h2>

          <div class="form-grid">
            <div class="form-field">
              <label class="label">Tên Combo</label>
              <input type="text" v-model="newCombo.name" placeholder="Nhập tên combo..." />
            </div>

            <div class="form-field">
              <label class="label">Trạng thái</label>
              <label class="switch-row">
                <input type="checkbox" v-model="newCombo.isAvailable" />
                <span>Còn hàng</span>
              </label>
            </div>
          </div>

          <div class="form-field full-width" style="margin-bottom: 16px;">
            <label class="label">Mô tả chi tiết</label>
            <textarea 
              v-model="newCombo.description" 
              rows="3" 
              placeholder="Nhập thành phần, hương vị..." 
              class="input-textarea">
            </textarea>
          </div>

          <strong class="label">Chọn món:</strong>
          <div class="checkbox-container">
            <div v-for="food in foods" :key="food.id" class="checkbox-item">
              <input
                type="checkbox"
                :checked="isSelected(food.id)"
                @change="toggleFood(food)"
              />
              <span>{{ food.name }} ({{ food.price.toLocaleString('vi-VN') }} VND)</span>
            </div>
          </div>

          <p class="auto-price">
            Giá combo tạm tính (giảm 11%):
            <strong>{{ computedPrice.toLocaleString("vi-VN") }} VND</strong>
          </p>

          <div class="form-image">
            <label class="label">Hình ảnh</label>
            <input type="file" @change="onFileChange" accept="image/*" />

            <div class="preview-img" v-if="previewImage || newCombo.imageUrl">
              <img :src="previewImage || newCombo.imageUrl || '/placeholder-image.png'" alt="Preview" />
            </div>
          </div>

          <div class="modal-actions">
            <button class="btn-cancel" @click="closeModal()">Hủy</button>
            <button class="btn-save" @click="saveCombo">Lưu</button>
          </div>

        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import api from "@/api";

// State
const foods = ref([]);
const combos = ref([]);
const showModal = ref(false);
const previewImage = ref(null);
const editCombo = ref(null);

const newCombo = ref({
  name: "",
  description: "",
  isAvailable: true,
  items: [],
  image: null,
  imageUrl: null,
});

// Utils
const getComboImage = (combo) => {
  if (!combo.imageUrl) return '/placeholder-image.png';
  return combo.imageUrl.startsWith('http') ? combo.imageUrl : `https://localhost:7119${combo.imageUrl}`;
};

const onImgError = e => { e.target.src = '/placeholder-image.png'; };

// Fetch API
const fetchFoods = async () => {
  try { const res = await api.get("/Food"); foods.value = res.data; } 
  catch (e) { console.error(e); }
};

const fetchCombos = async () => {
  try {
    const res = await api.get("/Combo");
    const list = res.data;
    await Promise.all(list.map(async combo => {
      try {
        const detail = await api.get(`/Combo/${combo.id}`);
        combo.items = detail.data.foods || [];
        combo.deletedAt = detail.data.deletedAt || null;
        combo.description = detail.data.description;
        combo.imageUrl = detail.data.imageUrl;
        combo.isAvailable = detail.data.isAvailable ?? true;
      } catch (err) {
        combo.items = [];
      }
    }));
    combos.value = list;
  } catch (e) { console.error(e); }
};

// Logic Form
const isSelected = foodId => newCombo.value.items.some(i => i.foodId === foodId);

const toggleFood = food => {
  const idx = newCombo.value.items.findIndex(i => i.foodId === food.id);
  if (idx >= 0) newCombo.value.items.splice(idx, 1);
  else newCombo.value.items.push({ foodId: food.id, quantity: 1 });
};

const computedPrice = computed(() => {
  let total = 0;
  newCombo.value.items.forEach(item => {
    const food = foods.value.find(f => f.id === item.foodId);
    if (food) total += food.price * item.quantity;
  });
  return Math.round(total * 0.89);
});

const onFileChange = e => {
  const file = e.target.files[0];
  if (!file) return;
  newCombo.value.image = file;
  const reader = new FileReader();
  reader.onload = ev => previewImage.value = ev.target.result;
  reader.readAsDataURL(file);
};

const openModal = async comboId => {
  if (comboId) {
    editCombo.value = comboId;
    try {
      const res = await api.get(`/Combo/${comboId}`);
      const data = res.data;
      newCombo.value = {
        name: data.name,
        description: data.description || "",
        isAvailable: data.isAvailable ?? true,
        items: data.foods.map(f => ({ foodId: f.foodId, quantity: f.quantity })),
        image: null,
        imageUrl: data.imageUrl,
      };
      previewImage.value = data.imageUrl;
    } catch (e) {
      alert("Lỗi tải thông tin combo!"); console.error(e); return;
    }
  } else {
    editCombo.value = null;
    newCombo.value = { name: "", description: "", isAvailable: true, items: [], image: null, imageUrl: null };
    previewImage.value = null;
  }
  showModal.value = true;
};

const closeModal = () => { showModal.value = false; previewImage.value = null; };

// Lưu combo
const saveCombo = async () => {
  if (!newCombo.value.name.trim()) { alert("Tên combo không được để trống!"); return; }

  const fd = new FormData();
  fd.append("Name", newCombo.value.name);
  fd.append("Description", newCombo.value.description || "");
  fd.append("Price", computedPrice.value);
  fd.append("IsAvailable", newCombo.value.isAvailable);

  if (newCombo.value.image && newCombo.value.image instanceof File)
    fd.append("Image", newCombo.value.image);

  if (newCombo.value.items.length)
    newCombo.value.items.forEach((item, i) => {
      fd.append(`Foods[${i}].FoodId`, item.foodId);
      fd.append(`Foods[${i}].Quantity`, item.quantity);
    });

  try {
    if (editCombo.value)
      await api.put(`/Combo/${editCombo.value}`, fd, { headers: { "Content-Type": "multipart/form-data" } });
    else
      await api.post(`/Combo`, fd, { headers: { "Content-Type": "multipart/form-data" } });
    
    alert("Lưu thành công!");
    showModal.value = false;
    fetchCombos();
  } catch (err) {
    console.error(err);
    alert("Lưu thất bại!");
  }
};

// Xóa / Khôi phục
const deleteCombo = async id => { if (confirm("Xóa combo?")) { await api.delete(`/Combo/${id}`); fetchCombos(); } };
const restoreCombo = async id => { if (confirm("Khôi phục combo?")) { await api.put(`/Combo/restore/${id}`); fetchCombos(); } };

// Mounted
onMounted(() => { fetchFoods(); fetchCombos(); });
</script>

<style scoped>
/* ===============================
   PAGE LAYOUT
================================ */
.page-wrapper {
  padding: 20px;
  min-height: 100vh;
  background: #f9f9f9;
}

.content-container {
  max-width: 1200px;
  margin: auto;
}

/* ===============================
   HEADER
================================ */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.header h1 {
  color: #ff4f8e;
  font-size: 28px;
  font-weight: 700;
}

/* ===============================
   BUTTON ADD
================================ */
.btn-add {
  background: #ff6fa5;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 600;
  transition: 0.2s;
}

.btn-add:hover {
  background: #ff4f8e;
}

/* ===============================
   GRID LIST
================================ */
.grid-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 20px;
}

/* ===============================
   FOOD CARD
================================ */
.food-card {
  background: white;
  border-radius: 16px;
  border: 1px solid #ffd6e8;
  box-shadow: 0 6px 18px rgba(255, 150, 180, 0.15);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: transform 0.2s;
}

.food-card:hover {
  transform: translateY(-5px);
}

.food-img {
  width: 100%;
  height: 180px;
  object-fit: cover;
  background: #ffeef5;
}

/* ===============================
   CARD INFO
================================ */
.info {
  padding: 15px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.name {
  font-weight: 700;
  color: #ff4f8e;
  margin-bottom: 6px;
  font-size: 18px;
}

.price {
  color: #ff2f70;
  font-weight: 600;
  margin-bottom: 8px;
  font-size: 16px;
}

.description-text {
  font-size: 13px;
  color: #666;
  margin-bottom: 8px;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.status {
  font-weight: 600;
  margin-bottom: 6px;
  font-size: 14px;
}

.items {
  font-size: 14px;
  margin-bottom: 12px;
  color: #555;
}

/* ===============================
   ACTION BUTTONS
================================ */
.action-buttons {
  display: flex;
  gap: 8px;
  margin-top: auto;
}

.btn-edit,
.btn-delete,
.btn-restore {
  flex: 1;
  padding: 8px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
}

.btn-edit {
  background: #ffd659;
  color: #7a5e00;
}

.btn-edit:hover {
  background: #ffca2c;
}

.btn-delete {
  background: #ff8097;
  color: white;
}

.btn-delete:hover {
  background: #ff4f70;
}

.btn-restore {
  background: #8ee68c;
  color: #145214;
}

/* ===============================
   MODAL
================================ */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
  backdrop-filter: blur(2px);
}

.modal-box {
  background: white;
  width: 800px;
  max-width: 95%;
  max-height: 64vh;
  padding: 24px;
  border-radius: 20px;
  overflow-y: auto;
  box-shadow: 0 10px 30px rgba(255, 80, 140, 0.3);
  border: 2px solid #fff0f5;
}

.modal-title {
  font-size: 24px;
  font-weight: 700;
  color: #ff4f8e;
  margin-bottom: 20px;
  text-align: center;
}

/* ===============================
   FORM
================================ */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 180px;
  gap: 16px;
  margin-bottom: 12px;
}

.form-field {
  display: flex;
  flex-direction: column;
}

.full-width {
  width: 100%;
}

.label {
  font-weight: 600;
  color: #ff4f8e;
  margin-bottom: 6px;
  font-size: 14px;
}

input[type="text"],
input[type="file"],
.input-textarea {
  padding: 10px;
  border: 1px solid #ffc9dd;
  border-radius: 10px;
  outline: none;
  font-size: 14px;
  transition: border 0.2s;
}

input:focus,
.input-textarea:focus {
  border-color: #ff4f8e;
}

.input-textarea {
  resize: vertical;
  font-family: inherit;
}

/* ===============================
   SWITCH & CHECKBOX
================================ */
.switch-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
  cursor: pointer;
  user-select: none;
}

.switch-row input {
  width: 18px;
  height: 18px;
  accent-color: #ff4f8e;
}

.checkbox-container {
  max-height: 150px;
  overflow-y: auto;
  border: 1px solid #ffc9dd;
  border-radius: 10px;
  padding: 10px;
  margin-bottom: 16px;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px 12px;
  background: #fffcfd;
}

.checkbox-item {
  display: flex;
  align-items: center;
  font-size: 14px;
}

.checkbox-item input {
  margin-right: 8px;
  width: 16px;
  height: 16px;
  accent-color: #ff4f8e;
}

/* ===============================
   IMAGE PREVIEW
================================ */
.auto-price {
  font-size: 16px;
  margin: 10px 0;
  color: #ff4f8e;
  text-align: right;
}

.form-image {
  margin-top: 10px;
}

.preview-img {
  margin-top: 10px;
  text-align: center;
}

.preview-img img {
  max-width: 200px;
  max-height: 120px;
  border-radius: 10px;
  object-fit: cover;
  border: 1px solid #ffd6e8;
}

/* ===============================
   MODAL ACTIONS
================================ */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
  border-top: 1px solid #eee;
  padding-top: 16px;
}

.btn-cancel {
  background: #ffeef5;
  color: #ff4f8e;
  padding: 10px 20px;
  border-radius: 10px;
  cursor: pointer;
  border: none;
  font-weight: 600;
}

.btn-save {
  background: #ff4f8e;
  color: white;
  padding: 10px 24px;
  border-radius: 10px;
  cursor: pointer;
  border: none;
  font-weight: 600;
  box-shadow: 0 4px 10px rgba(255, 79, 142, 0.3);
}

.btn-save:hover {
  background: #e03e76;
}

/* ===============================
   SCROLLBAR
================================ */
.checkbox-container::-webkit-scrollbar,
.modal-box::-webkit-scrollbar {
  width: 6px;
}

.checkbox-container::-webkit-scrollbar-thumb,
.modal-box::-webkit-scrollbar-thumb {
  background-color: #ffd6e8;
  border-radius: 10px;
}
</style>

