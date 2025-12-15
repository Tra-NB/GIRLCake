<template>
  <div v-if="cartItems.length > 0" class="cart-container">

    <div class="cart-items">
      <div class="cart-item" v-for="item in cartItems" :key="item.id">
        <div class="img-wrapper">
            <img :src="item.image" :alt="item.name" class="cart-item-img" />
        </div>
        
        <div class="cart-item-info">
          <h3>{{ item.name }}</h3>
          <p class="cart-item-desc">{{ item.description }}</p>
          
          <div class="cart-item-actions">
            <div class="quantity-control">
              <button @click="decreaseQty(item)" :disabled="item.quantity <= 1">
                <i class="bi bi-dash"></i> - 
              </button>
              <input type="number" v-model.number="item.quantity" @input="validateQuantity(item)" min="1" max="20"/>
              <button @click="increaseQty(item)" :disabled="item.quantity >= 20">
                <i class="bi bi-plus"></i> +
              </button>
            </div>
            <span class="item-price">{{ formatCurrency(item.price * item.quantity) }}</span>
          </div>
        </div>
        <button class="btn-remove" @click="removeItem(item.id)" title="Xóa sản phẩm">
            &times;
        </button>
      </div>
    </div>

    <div class="cart-summary">
      <h2 class="summary-title">Thanh Toán</h2>

      <div class="summary-details">
          <div class="summary-row">
            <span>Số sản phẩm</span>
            <span class="fw-bold">{{ totalItems }}</span>
          </div>

          <div class="summary-row">
            <span>Tạm tính</span>
            <span class="fw-bold">{{ formatCurrency(subTotal) }}</span>
          </div>

          <div class="summary-row">
            <span>Phí vận chuyển</span>
            <span v-if="freeShipApplied" class="text-success">Miễn phí</span>
            <span v-else>{{ formatCurrency(shippingFeeFromAPI) }}</span>
          </div>
      </div>

      <div class="voucher-section">
        <label class="voucher-label">Mã giảm giá</label>
        
        <div v-if="freeShipApplied" class="voucher-applied success-bg">
          <div class="voucher-text">
             <span><i class="bi bi-check-circle-fill"></i> Mã <strong>{{ freeShipCode }}</strong></span>
             <small>Đã miễn phí vận chuyển</small>
          </div>
          <button type="button" class="btn-icon-remove" @click="removeFreeShipVoucher">&times;</button>
        </div>

        <template v-else>
          <div class="voucher-input-group">
              <input type="text" v-model="manualVoucher" placeholder="Nhập mã (VD: FREESHIP15)" :disabled="voucherApplied"/>
              <button type="button" @click="applyVoucher" :disabled="voucherApplied">Áp dụng</button>
          </div>
          
          <select v-model="voucherCode" :disabled="voucherApplied" class="voucher-select">
            <option value="">-- Chọn voucher có sẵn --</option>
            <option v-for="v in voucherList" :key="v.id" :value="v.code" :disabled="isVoucherUsed(v.id.toString())" 
              :class="{ 'used-voucher': isVoucherUsed(v.id.toString()) }">
              {{ v.code }} {{ v.isPercent ? `(-${v.discountValue}%)` : `(-${formatCurrency(v.discountValue)})` }}
            </option>
          </select>
        </template>

        <div v-if="voucherApplied && appliedVoucher.code !== freeShipCode" class="voucher-applied discount-bg">
           <div class="voucher-text">
             <span><i class="bi bi-tag-fill"></i> Voucher: <strong>{{ voucherDescription }}</strong></span>
             <small>Giảm: <span v-if="appliedVoucher?.isPercent">{{ appliedVoucher.discountValue }}%</span>
             <span v-else>{{ formatCurrency(productDiscount) }}</span></small>
           </div>
           <button class="btn-icon-remove" @click="removeVoucher">&times;</button>
        </div>
      </div>

      <div class="summary-divider"></div>

      <div class="summary-row total-row">
        <span>Tổng cộng</span>
        <span class="total-price">{{ formatCurrency(totalPrice) }}</span>
      </div>

      <div class="customer-info">
        <h3 class="form-title">Thông tin giao hàng</h3>
        <div class="checkout-form">
            <div class="form-group">
                <input type="text" v-model="customer.name" placeholder=" " id="c-name" />
                <label for="c-name">Họ và tên</label>
            </div>
            <div class="form-group">
                <input type="text" v-model="customer.phone" placeholder=" " id="c-phone" @input="validatePhone" />
                <label for="c-phone">Số điện thoại</label>
            </div>
            <div class="form-group">
                <input type="text" v-model="customer.address" placeholder=" " id="c-addr" />
                <label for="c-addr">Địa chỉ nhận hàng</label>
            </div>
            <div class="form-group">
                <select v-model="customer.payment">
                    <option value="cod">Thanh toán khi nhận hàng (COD)</option>
                    <option value="momo">Ví MoMo</option>
                </select>
            </div>
        </div>
      </div>

      <button class="btn-checkout" :disabled="!isCustomerValid || totalItems === 0" @click="checkout">
        Xác Nhận Đặt Hàng
      </button>
    </div>
  </div>

  <div v-else class="empty-cart">
    <div class="empty-icon">🍰</div>
    <p>Giỏ hàng của bạn đang trống trơn nè!</p>
    <router-link to="/home" class="btn-continue">Đi mua bánh thôi</router-link>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import api from "@/api";

const cartItems = ref([]);
const customer = ref({ name: "", phone: "", address: "", payment: "cod" });
const userId = ref("");

const voucherCode = ref("");
const manualVoucher = ref("");
const voucherList = ref([]);

const voucherApplied = ref(false);
const productDiscount = ref(0); 
const voucherDescription = ref("");
const appliedVoucher = ref(null);

const freeShipApplied = ref(false);
const freeShipCode = ref("FREESHIP15");

const orders = ref([]);
const usedVoucherIds = ref([]);

// --- Computed ---
const totalItems = computed(() => cartItems.value.reduce((acc, i) => acc + i.quantity, 0));
const subTotal = computed(() => cartItems.value.reduce((acc, i) => acc + i.price * i.quantity, 0));
const shippingFeeFromAPI = ref(0);
const totalPrice = computed(() => {
  let discount = 0;
  let shippingFee = shippingFeeFromAPI.value || 0;

  if (appliedVoucher.value?.code === freeShipCode.value) {
    discount = 0;
    shippingFee = shippingFeeFromAPI.value || 0; 
  } else if (appliedVoucher.value) {
    discount = productDiscount.value;
  }

  return Math.max(subTotal.value - discount + shippingFee, 0);
});

const isCustomerValid = computed(() => customer.value.name.trim() !== "" &&
                                      customer.value.address.trim() !== "" &&
                                      /^\d{10}$/.test(customer.value.phone));

// --- Helpers ---
// ĐÃ SỬA: Thay "đ" thành "VND"
const formatCurrency = (v) => (Number(v) || 0).toLocaleString("vi-VN") + " VND";
const findImagePath = (path) => path ? `https://localhost:7119${path}` : "/images/default.png";

// --- Cart ---
const fetchCart = async () => {
  try {
    const token = localStorage.getItem("token");
    const res = await api.get("/Cart", { headers: { Authorization: `Bearer ${token}` } });
    cartItems.value = res.data.items?.map(i => ({
      id: i.productId,
      name: i.productName,
      price: i.price,
      quantity: i.quantity,
      description: i.description || "",
      image: findImagePath(i.imageUrl)
    })) || [];

    shippingFeeFromAPI.value = res.data.shippingFee || 15000;
  } catch {}
};

const saveCartItem = async (item) => {
  try {
    const token = localStorage.getItem("token");
    await api.post("/Cart/product", { productId: item.id, quantity: item.quantity }, { headers: { Authorization: `Bearer ${token}` } });
  } catch {}
};

const increaseQty = (i) => { if(i.quantity<20){i.quantity++; saveCartItem(i);} };
const decreaseQty = (i) => { if(i.quantity>1){i.quantity--; saveCartItem(i);} };
const validateQuantity = (i) => { if(i.quantity<1)i.quantity=1; if(i.quantity>20)i.quantity=20; saveCartItem(i); };
const removeItem = async (id) => { 
  const token = localStorage.getItem("token");
  await api.delete(`/Cart/product/${id}`, { headers: { Authorization: `Bearer ${token}` } });
  cartItems.value = cartItems.value.filter(i=>i.id!==id);
};

// --- User ---
const fetchCurrentUser = async () => {
  const token = localStorage.getItem("token");
  if(!token) return;
  try {
    const res = await api.get("/Users/me", { headers:{Authorization:`Bearer ${token}`} });
    userId.value = res.data.id;
    customer.value.name = res.data.fullName || "";
    customer.value.address = res.data.address || "";
    customer.value.phone = res.data.phone || "";
  } catch {}
};

const validatePhone = () => { customer.value.phone = customer.value.phone.replace(/\D/g,""); };

// --- Voucher ---
const fetchUsedVouchers = async () => {
  const token = localStorage.getItem("token");
  if(!token || !userId.value) return;
  try {
    const res = await api.get(`/Order/user/${userId.value}`, { headers:{Authorization:`Bearer ${token}`} });
    orders.value = res.data || [];
    usedVoucherIds.value = orders.value.filter(o=>o.VoucherId).map(o=>o.VoucherId);
  } catch {}
};

const isVoucherUsed = (voucherId) => usedVoucherIds.value.includes(voucherId);

const fetchActiveVouchers = async () => {
  const res = await api.get("/Voucher/active");
  voucherList.value = res.data.map(v => ({
    id: v.id,
    code: v.code,
    description: v.description,
    discountValue: v.discountValue,
    isPercent: v.discountType === 0
  })).filter(v => v.code !== freeShipCode.value);
};

const applyVoucher = () => {
  const code = manualVoucher.value || voucherCode.value;
  if (!code) return alert("Hãy nhập hoặc chọn voucher!");

  if (code === freeShipCode.value) {
    freeShipApplied.value = true;
    voucherApplied.value = true;
    appliedVoucher.value = { code: freeShipCode.value, id: 0 };
    productDiscount.value = 0; 
    voucherDescription.value = freeShipCode.value;
    return;
  }

  const voucher = voucherList.value.find(v => v.code === code);
  if (!voucher) return alert("Voucher không tồn tại!");
  if (isVoucherUsed(voucher.id.toString())) return alert("Voucher này đã dùng rồi!");

  appliedVoucher.value = voucher;
  voucherApplied.value = true;
  voucherDescription.value = voucher.code;

  productDiscount.value = voucher.isPercent 
                          ? Math.round(subTotal.value * voucher.discountValue / 100)
                          : voucher.discountValue;
};

const removeVoucher = () => {
  voucherApplied.value = false;
  appliedVoucher.value = null;
  manualVoucher.value = "";
  voucherCode.value = "";
  productDiscount.value = 0;
  voucherDescription.value = "";
};

const removeFreeShipVoucher = () => {
  freeShipApplied.value = false;
  manualVoucher.value = "";
  voucherCode.value = "";
};

// --- Checkout ---
const checkout = async () => {
  const token = localStorage.getItem("token");
  if (!token) return alert("Bạn chưa đăng nhập!");

  const payload = {
    ShippingAddress: customer.value.address,
    Status: "Pending",
    PaymentMethod: customer.value.payment === "momo" ? "Momo" : "CashOnDelivery",
    DiscountAmount: productDiscount.value, 
    ShippingFee: shippingFeeFromAPI.value,  
    TotalAmount: totalPrice.value,
    UserId: userId.value,
    UserName: customer.value.name,
    VoucherId: appliedVoucher.value?.id || null,
    VoucherCode: appliedVoucher.value?.code || null,
    OrderDetails: cartItems.value.map(i => ({ ProductId: i.id, Quantity: i.quantity, UnitPrice: i.price }))
  };

  try {
    await api.post("/Order", payload, { headers: { Authorization: `Bearer ${token}` } });
    alert("Đặt hàng thành công!");
    await api.delete("/Cart/cart", { headers: { Authorization: `Bearer ${token}` } });
    cartItems.value = [];
    removeVoucher();
    removeFreeShipVoucher();
  } catch (err) {
    console.error(err);
    alert("Có lỗi xảy ra khi đặt hàng!");
  }
};

onMounted(async () => {
  await fetchCurrentUser();
  await fetchCart();
  await fetchUsedVouchers();
  await fetchActiveVouchers();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;500;600;700&display=swap');

/* --- GENERAL LAYOUT --- */
.cart-container {
  display: flex;
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
  font-family: 'Quicksand', sans-serif;
  padding: 30px 20px;
  /* ĐÃ SỬA: Thêm nền Gradient hồng nhạt thay vì transparent */
  background: linear-gradient(135deg, #fff0f5 0%, #ffe6f0 100%);
  border-radius: 20px;
  margin-top: 20px;
}

/* --- LEFT SIDE: CART ITEMS --- */
.cart-items {
  flex: 1.8;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.cart-item {
  display: flex;
  align-items: center;
  /* ĐÃ SỬA: Nền hồng rất nhạt thay vì trắng tinh */
  background: #fff9fc;
  border-radius: 20px;
  padding: 15px;
  box-shadow: 0 5px 20px rgba(255, 182, 193, 0.2); 
  /* ĐÃ SỬA: Viền hồng rõ hơn */
  border: 1px solid #ffd1e0;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  position: relative;
}

.cart-item:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(255, 79, 142, 0.25);
  border-color: #ff9cbd;
}

.img-wrapper {
  width: 100px;
  height: 100px;
  border-radius: 15px;
  overflow: hidden;
  margin-right: 20px;
  border: 1px solid #ffdeeb;
}

.cart-item-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cart-item-info {
  flex: 1;
}

.cart-item-info h3 {
  color: #d6336c; /* Hồng đậm hơn cho tiêu đề */
  margin: 0 0 5px 0;
  font-size: 1.1rem;
  font-weight: 700;
}

.cart-item-desc {
  font-size: 0.85rem;
  color: #995c73; /* Chữ xám ánh hồng */
  margin-bottom: 15px;
  line-height: 1.4;
}

/* Actions & Quantity */
.cart-item-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.quantity-control {
  display: flex;
  align-items: center;
  /* ĐÃ SỬA: Nền control màu hồng nhạt */
  background: #fff0f6;
  border: 1px solid #ffb3c9;
  border-radius: 12px;
  padding: 3px;
}

.quantity-control button {
  background: white;
  border: none;
  color: #ff4f8e;
  width: 28px;
  height: 28px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: bold;
  box-shadow: 0 2px 5px rgba(255, 79, 142, 0.1);
  transition: all 0.2s;
}

.quantity-control button:hover:not(:disabled) {
  background: #ff4f8e;
  color: white;
}

.quantity-control button:disabled {
  color: #ffc2d6;
  cursor: default;
}

.quantity-control input {
  width: 40px;
  text-align: center;
  border: none;
  background: transparent;
  font-weight: 600;
  color: #d6336c;
  outline: none;
}

/* Price */
.item-price {
  font-weight: 800;
  color: #ff4f8e;
  font-size: 1.1rem;
}

/* Remove Button */
.btn-remove {
  background: #ffe6f0;
  border: none;
  color: #ff4f8e;
  font-size: 1.5rem;
  line-height: 1;
  cursor: pointer;
  width: 35px;
  height: 35px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: 15px;
  transition: all 0.2s;
}

.btn-remove:hover {
  background: #ff4f8e;
  color: white;
}

/* --- RIGHT SIDE: SUMMARY --- */
.cart-summary {
  flex: 1;
  /* ĐÃ SỬA: Nền hồng nhạt */
  background: #fff9fc;
  padding: 30px;
  border-radius: 24px;
  box-shadow: 0 10px 40px rgba(255, 79, 142, 0.15);
  height: fit-content;
  border: 1px solid #ffd1e0;
  display: flex;
  flex-direction: column;
}

.summary-title {
  color: #d6336c;
  font-size: 1.5rem;
  font-weight: 800;
  margin-bottom: 25px;
  text-align: center;
}

.summary-details {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin-bottom: 20px;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  color: #884d63; /* Text đậm màu hồng đất */
  font-size: 0.95rem;
}

.summary-row.total-row {
  font-size: 1.2rem;
  color: #d6336c;
  font-weight: 800;
  margin-top: 10px;
}

.total-price {
  color: #ff2f75;
  font-size: 1.5rem;
}

.fw-bold { font-weight: 600; color: #5c3041; }
.text-success { color: #28a745; font-weight: 600; }
.summary-divider { height: 1px; background: #ffcce0; margin: 15px 0; }

/* --- VOUCHER SECTION --- */
.voucher-section {
  background: #fff0f6;
  border-radius: 16px;
  padding: 20px;
  border: 1px dashed #ff9cbd;
  margin-bottom: 20px;
}

.voucher-label {
  display: block;
  font-weight: 700;
  color: #ff4f8e;
  margin-bottom: 10px;
  font-size: 0.9rem;
}

.voucher-input-group {
  display: flex;
  gap: 10px;
  margin-bottom: 10px;
}

.voucher-input-group input {
  flex: 1;
  padding: 10px 15px;
  border-radius: 10px;
  border: 1px solid #ffc2d6; /* Viền hồng */
  background: #fff;
  font-size: 0.9rem;
  outline: none;
  transition: border 0.3s;
  color: #d6336c;
}

.voucher-input-group input:focus {
  border-color: #ff4f8e;
  box-shadow: 0 0 0 3px rgba(255, 79, 142, 0.1);
}

.voucher-input-group button {
  padding: 0 15px;
  background: #ff7aa5;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  font-size: 0.85rem;
  transition: background 0.2s;
}

.voucher-input-group button:hover:not(:disabled) {
  background: #ff4f8e;
}

.voucher-select {
  width: 100%;
  padding: 10px;
  border-radius: 10px;
  border: 1px solid #ffc2d6;
  background: #fff;
  font-size: 0.9rem;
  color: #884d63;
  outline: none;
}

/* Voucher Applied Chips */
.voucher-applied {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  border-radius: 10px;
  margin-bottom: 8px;
}

.voucher-applied.success-bg { background: #e3f9e5; color: #1f7a2d; }
.voucher-applied.discount-bg { background: #ffdeeb; color: #d6336c; border: 1px solid #ffb3c9; }

.voucher-text {
  display: flex;
  flex-direction: column;
  font-size: 0.85rem;
}

.btn-icon-remove {
  background: transparent;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  opacity: 0.6;
  color: #d6336c;
}
.btn-icon-remove:hover { opacity: 1; }

/* --- FORM --- */
.customer-info { margin-top: 10px; }
.form-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #d6336c;
  margin-bottom: 15px;
}

.form-group {
  position: relative;
  margin-bottom: 15px;
}

.checkout-form input, .checkout-form select {
  width: 100%;
  padding: 12px 15px;
  /* ĐÃ SỬA: Viền và nền hồng nhạt */
  border: 2px solid #ffe6f0;
  border-radius: 12px;
  background: #fffafa;
  font-size: 0.95rem;
  outline: none;
  transition: all 0.3s;
  color: #5c3041;
}

.checkout-form input:focus, .checkout-form select:focus {
  background: #fff;
  border-color: #ff9cbd;
  box-shadow: 0 0 0 4px rgba(255, 79, 142, 0.1);
}

.form-group label {
  position: absolute;
  left: 15px;
  top: 50%;
  transform: translateY(-50%);
  color: #cc8ba0; /* Placeholder màu hồng đất nhạt */
  font-size: 0.9rem;
  pointer-events: none;
  transition: 0.2s;
  background: transparent;
}

.checkout-form input:focus + label,
.checkout-form input:not(:placeholder-shown) + label {
  top: 0;
  left: 10px;
  font-size: 0.75rem;
  color: #ff4f8e;
  background: #fff9fc;
  padding: 0 5px;
  font-weight: 600;
}

/* --- CHECKOUT BUTTON --- */
.btn-checkout {
  width: 100%;
  padding: 16px;
  margin-top: 20px;
  background: linear-gradient(135deg, #ff4f8e 0%, #ff8fae 100%);
  color: white;
  border: none;
  border-radius: 14px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  box-shadow: 0 8px 20px rgba(255, 79, 142, 0.4);
  transition: all 0.3s;
}

.btn-checkout:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(255, 79, 142, 0.5);
  background: linear-gradient(135deg, #ff2f75 0%, #ff6f95 100%);
}

.btn-checkout:disabled {
  background: #ffccd8;
  color: #fff;
  box-shadow: none;
  cursor: not-allowed;
}

/* --- EMPTY CART --- */
.empty-cart {
  text-align: center;
  padding: 80px 20px;
  background: #fff0f5;
}
.empty-icon { font-size: 4rem; margin-bottom: 20px; animation: float 3s ease-in-out infinite; }
@keyframes float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }

.btn-continue {
  display: inline-block;
  margin-top: 20px;
  padding: 12px 30px;
  background: #ff4f8e;
  color: white;
  border-radius: 30px;
  text-decoration: none;
  font-weight: 700;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.3);
  transition: 0.2s;
}
.btn-continue:hover { transform: scale(1.05); }

/* --- RESPONSIVE --- */
@media (max-width: 900px) {
  .cart-container { flex-direction: column; }
  .cart-item { flex-direction: column; align-items: flex-start; }
  .img-wrapper { width: 100%; height: 180px; margin-bottom: 15px; margin-right: 0; }
  .btn-remove { position: absolute; top: 10px; right: 10px; background: rgba(255,255,255,0.8); }
}
</style>