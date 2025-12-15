<template>
  <div class="contact-wrapper">
    <div class="blob blob-1"></div>
    <div class="blob blob-2"></div>

    <div class="container main-container">
      <div class="contact-card shadow-lg">
        
        <div class="contact-info">
          <h2 class="info-title">Liên hệ</h2>
          <p class="info-desc">
            Bạn có thắc mắc hoặc cần tư vấn về các loại bánh? Đừng ngần ngại nhắn tin cho chúng mình nhé!
          </p>

          <div class="info-items">
            <div class="item">
              <div class="icon-box">
                <i class="bi bi-geo-alt-fill"></i>
              </div>
              <div class="text">
                <h5>Địa chỉ</h5>
                <p>123 Nguyễn Trãi, Hà Nội</p>
              </div>
            </div>

            <div class="item">
              <div class="icon-box">
                <i class="bi bi-envelope-fill"></i>
              </div>
              <div class="text">
                <h5>Email</h5>
                <p>support@girlcake.com</p>
              </div>
            </div>

            <div class="item">
              <div class="icon-box">
                <i class="bi bi-telephone-fill"></i>
              </div>
              <div class="text">
                <h5>Hotline</h5>
                <p>0123 456 789</p>
              </div>
            </div>
            
            <div class="item mt-4">
               <div class="text">
                <h5>Giờ mở cửa</h5>
                <p class="mb-0">Thứ 2 – Thứ 7: 8:00 - 21:00</p>
                <p>Chủ nhật: Nghỉ</p>
              </div>
            </div>
          </div>

          <div class="social-icons">
            <a href="#"><i class="bi bi-facebook"></i></a>
            <a href="#"><i class="bi bi-instagram"></i></a>
            <a href="#"><i class="bi bi-twitter"></i></a>
          </div>
        </div>

        <div class="contact-form">
          <h3 class="form-title">Gửi tin nhắn</h3>
          
          <form @submit.prevent="submitForm">
            <div class="input-group-wrapper">
              <label>Họ và tên</label>
              <input 
                type="text" 
                class="modern-input" 
                placeholder="Nhập tên của bạn" 
                v-model="form.name" 
              />
            </div>

            <div class="input-group-wrapper">
              <label>Email</label>
              <input 
                type="email" 
                class="modern-input" 
                placeholder="email@example.com" 
                v-model="form.email" 
              />
            </div>

            <div class="input-group-wrapper">
              <label>Nội dung</label>
              <textarea
                class="modern-input"
                rows="4"
                placeholder="Bạn muốn nhắn nhủ điều gì..."
                v-model="form.message"
              ></textarea>
            </div>

            <button class="btn btn-submit-glow">
              Gửi ngay <i class="bi bi-send-fill ms-2"></i>
            </button>
          </form>
        </div>

      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      form: {
        name: "",
        email: "",
        message: "",
      },
    };
  },
  methods: {
    submitForm() {
      // Giữ nguyên logic cũ của bạn
      if(!this.form.name || !this.form.email) {
          alert("Vui lòng điền đầy đủ thông tin!");
          return;
      }
      alert(`Cảm ơn ${this.form.name}! Chúng tôi đã nhận được tin nhắn.`);
    },
  },
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;600;700;800&display=swap');

/* --- LAYOUT CHUNG --- */
.contact-wrapper {
  font-family: 'Quicksand', sans-serif;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff5f8; /* Nền hồng rất nhạt */
  position: relative;
  overflow: hidden;
  padding: 20px;
}

/* Hiệu ứng bong bóng trang trí nền */
.blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(40px);
  z-index: 0;
}
.blob-1 {
  width: 300px;
  height: 300px;
  background: rgba(255, 79, 142, 0.2);
  top: -50px;
  left: -50px;
}
.blob-2 {
  width: 400px;
  height: 400px;
  background: rgba(255, 182, 193, 0.3);
  bottom: -100px;
  right: -100px;
}

.main-container {
  z-index: 1;
  max-width: 1000px;
  width: 100%;
}

/* --- CARD CHÍNH --- */
.contact-card {
  display: flex;
  flex-wrap: wrap;
  background: #ffffff;
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 15px 40px rgba(255, 79, 142, 0.15);
}

/* --- CỘT TRÁI (INFO) --- */
.contact-info {
  flex: 1;
  min-width: 300px;
  background: linear-gradient(135deg, #ff4f8e 0%, #ff8fae 100%);
  padding: 50px 40px;
  color: white;
  display: flex;
  flex-direction: column;
  justify-content: center;
  position: relative;
}

/* Họa tiết trang trí cột trái */
.contact-info::before {
  content: "";
  position: absolute;
  width: 150px;
  height: 150px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  top: -30px;
  right: -30px;
}

.info-title {
  font-weight: 800;
  font-size: 2.2rem;
  margin-bottom: 15px;
}

.info-desc {
  font-size: 0.95rem;
  opacity: 0.9;
  margin-bottom: 40px;
  line-height: 1.6;
}

.item {
  display: flex;
  align-items: center;
  margin-bottom: 25px;
}

.icon-box {
  width: 45px;
  height: 45px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  margin-right: 15px;
  flex-shrink: 0;
}

.text h5 {
  font-size: 1rem;
  font-weight: 700;
  margin: 0;
}

.text p {
  font-size: 0.9rem;
  margin: 0;
  opacity: 0.9;
}

.social-icons {
  margin-top: auto;
  display: flex;
  gap: 15px;
}

.social-icons a {
  color: white;
  font-size: 1.2rem;
  transition: transform 0.2s;
}

.social-icons a:hover {
  transform: translateY(-3px);
}

/* --- CỘT PHẢI (FORM) --- */
.contact-form {
  flex: 1.3;
  min-width: 320px;
  padding: 50px 40px;
  background: #fff;
}

.form-title {
  color: #ff4f8e;
  font-weight: 800;
  font-size: 1.8rem;
  margin-bottom: 30px;
}

.input-group-wrapper {
  margin-bottom: 20px;
}

.input-group-wrapper label {
  display: block;
  font-weight: 600;
  color: #666;
  margin-bottom: 8px;
  font-size: 0.9rem;
}

.modern-input {
  width: 100%;
  padding: 12px 15px;
  border: 2px solid #f0f0f0;
  border-radius: 12px;
  background: #fdfdfd;
  font-size: 0.95rem;
  transition: all 0.3s ease;
  font-family: 'Quicksand', sans-serif;
  color: #333;
}

.modern-input:focus {
  outline: none;
  border-color: #ffb3c6;
  background: #fff;
  box-shadow: 0 0 0 4px rgba(255, 79, 142, 0.05);
}

.modern-input::placeholder {
  color: #ccc;
}

.btn-submit-glow {
  width: 100%;
  padding: 14px;
  background: linear-gradient(45deg, #ff4f8e, #ff8fae);
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
  margin-top: 10px;
}

.btn-submit-glow:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 79, 142, 0.5);
  background: linear-gradient(45deg, #ff2f70, #ff6f91);
}

/* --- RESPONSIVE --- */
@media (max-width: 768px) {
  .contact-card {
    flex-direction: column;
  }
  
  .contact-info {
    padding: 40px 25px;
  }
  
  .contact-form {
    padding: 40px 25px;
  }
}
</style>