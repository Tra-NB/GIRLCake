<template>
  <div class="page-wrapper">
    <div class="content-container">

      <div class="header">
        <h1>Quản lý món</h1>
        <button class="btn-add" @click="openAddModal">+ Thêm Món</button>
      </div>

      <div class="grid-container">
        <div v-for="food in foods" :key="food.id" class="food-card" :style="{ opacity: food.deletedAt ? 0.5 : 1 }">
          <img :src="resolveImage(food.imageUrl, food.categoryId)" class="food-img" />
          <div class="info">
            <h2 class="name" :title="food.name">{{ food.name }}</h2>
            <p class="desc">{{ food.description }}</p>
            <p class="price">{{ formatPrice(food.price) }} VND</p>
            <p class="category">Loại: {{ getCategoryName(food.categoryId) }}</p>
            <p class="status" :style="{ color: food.isAvailable ? 'green' : 'red', fontWeight: '600' }">
              {{ food.isAvailable ? "Còn hàng" : "Hết hàng" }}
            </p>
            <div class="action-buttons">
              <button class="btn-edit" @click="openEditModal(food)">Sửa</button>
              <button v-if="food.deletedAt !== null" class="btn-restore" @click="restoreFood(food)">Khôi phục</button>
              <button v-else class="btn-delete" @click="deleteFood(food)">Xóa</button>
            </div>
          </div>
        </div>
      </div>

      <div class="header" style="margin-top:30px;">
        <h1>Danh sách Loại Món</h1>
        <button class="btn-add" @click="openAddCategoryModal">+ Thêm Loại</button>
      </div>

      <table class="category-table">
        <thead>
          <tr>
            <th>Tên</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cat in categories" :key="cat.id" :style="{ opacity: cat.deletedAt ? 0.5 : 1 }">
            <td>{{ cat.name }}</td>
            <td>
              <button class="btn-edit" @click="openEditCategoryModal(cat)">Sửa</button>
              <button class="btn-delete" @click="deleteCategory(cat)">Xóa</button>
            </td>
          </tr>
        </tbody>
      </table>


    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-box">
        <h2 class="modal-title">{{ isEditing ? 'Sửa Món' : 'Thêm Món' }}</h2>
        <div class="form-group">
          <input v-model="form.name" placeholder="Tên món" />
          
          <div class="price-input-group">
            <input v-model.number="form.price" type="number" placeholder="Giá gốc (chưa +14%)" min="0" />
            <p class="price-note">
              *Lưu ý: Giá sẽ tự động tăng 14%.
            </p>
          </div>
          <textarea v-model="form.description" placeholder="Mô tả"></textarea>
          <select v-model="form.categoryId">
            <option :value="null">-- Chọn Loại Món --</option>
            <option v-for="c in categories.filter(c => c.deletedAt === null)" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
          <select v-model="form.isAvailable">
            <option :value="true">Còn hàng</option>
            <option :value="false">Hết hàng</option>
          </select>
          <input type="file" @change="onFileChange" />
          <div v-if="previewImage || form.imageUrl" class="preview-img">
            <img :src="previewImage ? previewImage : resolveImage(form.imageUrl, form.categoryId)" @error="onImgError" alt="Preview" />
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn-cancel" @click="closeModal">Hủy</button>
          <button class="btn-save" @click="saveFood">Lưu</button>
        </div>
      </div>
    </div>

    <div v-if="showCategoryModal" class="modal-overlay" @click.self="closeCategoryModal">
      <div class="modal-box">
        <h2 class="modal-title">{{ isEditingCategory ? 'Sửa Loại Món' : 'Thêm Loại Món' }}</h2>
        <div class="form-group">
          <input v-model="categoryForm.name" placeholder="Tên Category" />
        </div>
        <div class="modal-actions">
          <button class="btn-cancel" @click="closeCategoryModal">Hủy</button>
          <button class="btn-save" @click="saveCategory">Lưu</button>
        </div>
      </div>
    </div>

  </div>
</template>


<script setup>
import { ref, onMounted, computed } from "vue";
import api from "@/api";

// ==================== State ====================
// Foods
const foods = ref([]);
const showModal = ref(false);
const isEditing = ref(false);
const form = ref({
  id: null,
  name: "",
  price: 0, // Đặt giá trị mặc định là số
  description: "",
  imageUrl: "",
  isAvailable: true,
  categoryId: null // Đặt null
});
const selectedFile = ref(null);
const previewImage = ref("");

// Categories
const categories = ref([]);
const showCategoryModal = ref(false);
const isEditingCategory = ref(false);
const categoryForm = ref({ id: null, name: "" });

// ==================== Computed ====================
const getCategoryName = (categoryId) => {
  const c = categories.value.find(cat => cat.id === categoryId);
  return c ? c.name : "Chưa có loại";
};

// ==================== Fetch API ====================
const fetchFoods = async () => {
  try {
    const { data } = await api.get("/Food");
    foods.value = data.map(f => ({ ...f, deletedAt: f.deletedAt || null }));
  } catch (err) {
    console.error("Lỗi fetch foods:", err);
  }
};

const fetchCategories = async () => {
  try {
    const { data } = await api.get("/Category");
    categories.value = data.map(c => ({ ...c, deletedAt: c.deletedAt || null }));
  } catch (err) {
    console.error("Lỗi fetch categories:", err);
  }
};

// ==================== Utils ====================
const formatPrice = price => price ? Number(price).toLocaleString("vi-VN") : "0";

const resolveImage = (url) => {
  if (!url || url.trim() === "") return "/food/default.jpg";
  return url.startsWith("http") ? url : `https://localhost:7119${url}`;
};

const onImgError = e => { e.target.src = "/food/default.jpg"; };

// ==================== Modal Food ====================
const resetForm = () => {
    form.value = { id: null, name: "", price: 0, description: "", imageUrl: "", isAvailable: true, categoryId: null };
    selectedFile.value = null;
    previewImage.value = null;
};

const openAddModal = () => {
  isEditing.value = false;
  resetForm();
  showModal.value = true;
};

const openEditModal = food => {
  isEditing.value = true;
  // Khi chỉnh sửa, hiển thị giá trị đã lưu trong DB (đã cộng 14%)
  form.value = { ...food }; 
  previewImage.value = food.imageUrl ? resolveImage(food.imageUrl) : "/food/default.jpg";
  selectedFile.value = null;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  resetForm();
};

const onFileChange = e => {
  const file = e.target.files[0];
  if (file) {
    selectedFile.value = file;
    const reader = new FileReader();
    reader.onload = e => previewImage.value = e.target.result;
    reader.readAsDataURL(file);
  }
};

const saveFood = async () => {
  if (!form.value.categoryId) { alert("Chọn loại món!"); return; }
  if (!form.value.name.trim()) { alert("Nhập tên món!"); return; }
  if (form.value.price <= 0 || isNaN(form.value.price)) { alert("Giá món phải lớn hơn 0!"); return; }

  const formData = new FormData();
  formData.append("name", form.value.name);
  // GỬI GIÁ GỐC LÊN (API sẽ tự động cộng 14% trước khi lưu)
  formData.append("price", form.value.price); 
  formData.append("description", form.value.description);
  formData.append("categoryId", form.value.categoryId);
  formData.append("isAvailable", form.value.isAvailable);
  
  if (selectedFile.value) formData.append("FImageFile", selectedFile.value);

  try {
    if (isEditing.value)
      await api.put(`/Food/${form.value.id}`, formData, { headers: { "Content-Type": "multipart/form-data" } });
    else
      await api.post("/Food", formData, { headers: { "Content-Type": "multipart/form-data" } });

    await fetchFoods();
    closeModal();
  } catch (err) {
    console.error("Lỗi save food:", err);
    alert("Lưu món ăn thất bại!");
  }
};

const deleteFood = async (food) => {
  if (!confirm(`Xác nhận ẩn món ăn: ${food.name}?`)) return;
  try {
    await api.delete(`/Food/${food.id}`);
    food.deletedAt = new Date().toISOString();
    foods.value = [...foods.value];
  } catch (err) { 
      console.error(err); 
      alert("Xóa món ăn thất bại!");
  }
};

const restoreFood = async (food) => {
  try {
    await api.post(`/Food/restore/${food.id}`);
    food.deletedAt = null;
    foods.value = [...foods.value];
  } catch (err) { 
      console.error(err); 
      alert("Khôi phục món ăn thất bại!");
  }
};

// ==================== Modal Category ====================
const openAddCategoryModal = () => {
  isEditingCategory.value = false;
  categoryForm.value = { id: null, name: "" };
  showCategoryModal.value = true;
};

const openEditCategoryModal = cat => {
  isEditingCategory.value = true;
  categoryForm.value = { ...cat };
  showCategoryModal.value = true;
};

const closeCategoryModal = () => { showCategoryModal.value = false; };

const saveCategory = async () => {
  if (!categoryForm.value.name.trim()) { alert("Nhập tên category!"); return; }
  try {
    if (isEditingCategory.value)
      await api.put(`/Category/${categoryForm.value.id}`, categoryForm.value);
    else
      await api.post("/Category", categoryForm.value);

    await fetchCategories();
    closeCategoryModal();
  } catch (err) { 
      console.error(err);
      alert("Lưu loại món thất bại!");
  }
};

const deleteCategory = async (cat) => {
  const hasActiveFoods = foods.value.some(f => f.categoryId === cat.id && f.deletedAt === null);
  if (hasActiveFoods) {
    alert("Không thể xóa loại món này vì vẫn còn món đang hoạt động thuộc loại!");
    return;
  }
  if (!confirm(`Xác nhận xóa loại món: ${cat.name}?`)) return;

  try {
    // Giả định API thực hiện Soft Delete (gán DeletedAt)
    await api.delete(`/Category/${cat.id}`); 
    // Cập nhật trạng thái hiển thị
    const index = categories.value.findIndex(c => c.id === cat.id);
    if (index !== -1) categories.value[index].deletedAt = new Date().toISOString();
    categories.value = [...categories.value];
  } catch (err) {
    console.error(err);
    alert("Xóa loại món thất bại!");
  }
};


// ==================== Mounted ====================
onMounted(() => {
  fetchFoods();
  fetchCategories();
});
</script>


<style scoped>
/* ==================================================
   TỔNG THỂ
================================================== */
.page-wrapper {
  min-height: 100vh;
  padding: 30px;
  background: #f8f9fa;
}

.content-container {
  max-width: 1280px;
  margin: auto;
}

/* ==================================================
   HEADER
================================================== */
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 25px;
}

.header h1 {
  font-size: 32px;
  font-weight: 700;
  color: #ff5f8c;
}

/* Nút thêm */
.btn-add {
  padding: 10px 20px;
  border-radius: 14px;
  border: none;

  background: linear-gradient(135deg, #ff8ab9, #ff6fa5);
  color: #fff;
  font-weight: 600;

  cursor: pointer;
  box-shadow: 0 4px 10px rgba(255, 120, 160, 0.3);
  transition: transform 0.25s;
}

.btn-add:hover {
  transform: scale(1.05);
}

/* ==================================================
   GRID + CARD
================================================== */
.grid-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 22px;
}

.food-card {
  display: flex;
  flex-direction: column;

  background: #fff;
  border-radius: 20px;
  border: 1px solid #ffd6e8;
  padding-bottom: 10px;

  box-shadow: 0 8px 20px rgba(255, 150, 180, 0.18);
  transition: transform 0.25s;
}

.food-card:hover {
  transform: translateY(-6px);
}

.food-img {
  width: 100%;
  height: 180px;
  object-fit: cover;
  border-radius: 20px 20px 0 0;
}

/* ==================================================
   CARD INFO
================================================== */
.info {
  padding: 15px;
  display: flex;
  flex-direction: column;
}

.name {
  font-size: 18px;
  font-weight: 700;
  color: #ff4f8e;
  margin-bottom: 6px;

  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.desc {
  font-size: 14px;
  color: #777;

  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.price {
  margin: 6px 0;
  font-weight: 700;
  color: #ff2f70;
}

.status {
  font-weight: 600;
}

/* ==================================================
   ACTION BUTTONS
================================================== */
.action-buttons {
  display: flex;
  gap: 8px;
  margin-top: auto;
}

.btn-edit,
.btn-delete,
.btn-restore {
  padding: 6px 14px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  font-weight: 600;
}

.btn-edit {
  background: #ffd659;
}

.btn-delete {
  background: #ff8097;
  color: #fff;
}

.btn-restore {
  background: #8ee68c;
}

/* ==================================================
   MODAL OVERLAY (CUỘN NGOÀI)
================================================== */
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 999;

  display: flex;
  justify-content: center;
  align-items: center;

  padding: 20px;
  background: rgba(0, 0, 0, 0.35);
  backdrop-filter: blur(4px);

  /* Cho phép cuộn khi màn hình nhỏ */
  overflow-y: auto;
}

/* ==================================================
   MODAL BOX (CUỘN TRONG)
================================================== */
.modal-box {
  width: 420px;
  max-width: 95%;
  max-height: 65vh;

  padding: 25px;
  background: #fff;
  border-radius: 22px;
  border: 1px solid #ffd6e8;

  overflow-y: auto;
  box-shadow: 0 12px 35px rgba(255, 150, 180, 0.3);
  animation: modalFade 0.25s ease;
}

@keyframes modalFade {
  from {
    opacity: 0;
    transform: translateY(15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Scrollbar modal */
.modal-box::-webkit-scrollbar {
  width: 6px;
}
.modal-box::-webkit-scrollbar-thumb {
  background: #ff9fc2;
  border-radius: 10px;
}

/* ==================================================
   FORM
================================================== */
.modal-title {
  margin-bottom: 15px;
  font-size: 22px;
  font-weight: 700;
  color: #ff4f8e;
}

.form-group input,
.form-group textarea,
.form-group select {
  width: 100%;
  padding: 10px;
  margin-bottom: 12px;

  border-radius: 12px;
  border: 1px solid #ffc9dd;
}

.price-input-group {
  margin-bottom: 12px;
}

.price-note {
  margin-top: -6px;
  padding-left: 5px;
  font-size: 11.5px;
  color: #ff5f8c;
}

/* ==================================================
   MODAL ACTIONS
================================================== */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-cancel,
.btn-save {
  padding: 8px 16px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
}

.btn-cancel {
  background: #ffe2ed;
}

.btn-save {
  background: #ff7aa8;
  color: #fff;
}

/* ==================================================
   IMAGE PREVIEW
================================================== */
.preview-img {
  margin-top: 10px;
}

.preview-img img {
  max-width: 150px;
  max-height: 120px;
  border-radius: 8px;
  object-fit: cover;
}

/* ==================================================
   CATEGORY TABLE
================================================== */
.category-table {
  width: 100%;
  margin-bottom: 30px;
  border-collapse: collapse;
  background: #fff;
}

.category-table th,
.category-table td {
  padding: 12px;
  border: 1px solid #ffd6e8;
}

.category-table th {
  background: #ffe2ed;
  color: #ff4f8e;
}
</style>
