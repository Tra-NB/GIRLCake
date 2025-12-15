import './assets/main.css'
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import 'bootstrap-icons/font/bootstrap-icons.css';
import GoogleLoginPlugin from 'vue3-google-login'

import { createApp } from 'vue'
import App from './App.vue' 
import router from './router'

const app = createApp(App)
app.use(GoogleLoginPlugin,{
    clientId:'834360765832-kr0uv18gd6ngbtttr95b9eaet2kfstmo.apps.googleusercontent.com'
})

createApp(App).use(router).mount('#app')
