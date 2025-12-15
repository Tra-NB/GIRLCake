import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7119/api",
  headers: {
    "Content-Type": "application/json",
  },
});

/* ============================
   REQUEST INTERCEPTOR
   GẮN JWT TOKEN
============================ */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

/* ============================
   RESPONSE INTERCEPTOR
   XỬ LÝ 401 TOÀN CỤC
============================ */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.error("❌ 401 Unauthorized – Token không hợp lệ hoặc hết hạn");

      localStorage.removeItem("token");
      localStorage.removeItem("user");

      // redirect về login
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default api;
