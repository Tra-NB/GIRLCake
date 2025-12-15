import { createRouter, createWebHistory } from 'vue-router'

/* ===== Public pages ===== */
import Home from '../views/Home.vue'
import Combo from '../views/Combo.vue'
import Login from '../views/Auth/Login.vue'
import Register from '../views/Auth/Register.vue'
import Cart from '../views/Cart.vue'

/* ===== Routes ===== */
const routes = [
  /* ===== Redirect ===== */
  { 
    path: '/', 
    redirect: '/about'
  },

  /* ===== Home ===== */
  { 
    path: '/home', 
    component: Home 
  },

  /* ===== Combo list ===== */
  { 
    path: '/combo', 
    component: Combo 
  },

  /* ✅✅✅ COMBO DETAIL (BẮT BUỘC PHẢI CÓ) */
  {
    path: '/combo/:id',
    name: 'ComboDetail',
    component: () => import('../views/FoodDetail.vue') 
    // 👉 dùng chung trang chi tiết food/combo
  },

  /* ===== Public static pages ===== */
  { 
    path: "/about", 
    component: () => import("../views/About.vue") 
  },
  { 
    path: "/contact", 
    component: () => import("../views/Contact.vue") 
  },

  /* ===== Auth ===== */
  { 
    path: "/login", 
    name: "Login", 
    component: Login 
  },
  { 
    path: "/register", 
    name: "Register", 
    component: Register 
  },

  /* ===== Cart ===== */
  { 
    path: '/cart', 
    component: Cart 
  },

  /* ===== Food detail ===== */
  {
    path: "/food/:id",
    name: "FoodDetail",
    component: () => import("../views/FoodDetail.vue"),
  },

  /* ===== User profile ===== */
  {
    path: "/account",
    name: "Account",
    component: () => import("../views/Account.vue"),
  },

  /* ===== Admin pages ===== */
  {
    path: "/manageFoods",
    name: "ManageFoods",
    component: () => import("../views/Management/ManageFoods.vue"),
  },
  {
    path: "/manageCombos",
    name: "ManageCombos",
    component: () => import("../views/Management/ManageCombos.vue"),
  },
  {
    path: "/manageOrders",
    name: "ManageOrders",
    component: () => import("../views/Management/ManageOrders.vue"),
  },
  {
    path: "/manageUsers",
    name: "ManageUsers",
    component: () => import("../views/Management/ManageUsers.vue"),
  },
  {
    path: "/manageVouchers",
    name: "ManageVouchers",
    component: () => import("../views/Management/ManageVouchers.vue"),
  },
  {
    path: "/manageReviews",
    name: "ManageReviews",
    component: () => import("../views/Management/ManageReviews.vue"),
  }
]

/* ===== Router ===== */
const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
