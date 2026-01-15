
# 🕊️ Kabutar Backend

Welcome to the **Kabutar Backend**! This is the server-side powerhouse for **Kabutar Messenger**, a simple and efficient messaging platform. Built with the latest **.NET 8**, it provides robust APIs and real-time communication for a seamless user experience. 🚀

---

## ✨ Features
- 🔐 **Secure Authentication**: User registration and login with JWT.
- 💬 **1-on-1 Messaging**: Real-time chats with SignalR.
- 📎 **File Attachments**: Attach and share files in messages.
- 🖼️ **User Profiles**: Customize profile pictures and "About" info.
- 🗄️ **Reliable Storage**: PostgreSQL database for scalable data management.
- 🛡️ **Rate Limiting**: Protection against brute force attacks.
- 🔒 **Environment Variables**: Secure configuration management.
- 📧 **Email Verification**: Verify user emails with OTP codes.
- ⏰ **Last Active**: Track user activity timestamps.

---

## 🛠️ Technology Stack
- **🖥️ Backend Framework**: ASP.NET Core Web API (using .NET 8)
- **💾 Database**: PostgreSQL
- **📡 Real-Time Communication**: SignalR
- **🔑 Authentication**: JSON Web Tokens (JWT)
- **📘 ORM**: Entity Framework Core

---

## 📋 Prerequisites
Before running the project, ensure you have:
- ✅ **.NET 8 SDK**
- ✅ **PostgreSQL**
- ✅ **Visual Studio 2022** or another preferred IDE

---

## 🚀 Getting Started

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/keldibekovulugbek/kabutar-backend.git
cd kabutar-backend
```

### 2️⃣ Configure Environment Variables
- Copy `.env.example` to `.env`
- Update the values in `.env` with your configuration:
```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=kabutar-db
DB_USER=postgres
DB_PASSWORD=your_password

JWT_KEY=your-secret-jwt-key-here-minimum-32-characters
EMAIL_ADDRESS=your-email@gmail.com
EMAIL_PASSWORD=your-gmail-app-password

CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

### 3️⃣ Set Up the Database
- Make sure PostgreSQL is running
- Create database: `createdb kabutar-db`

### 4️⃣ Apply Migrations
```bash
dotnet ef database update
```

### 5️⃣ Run the Application
Start the backend with:
```bash
dotnet run
```
Your backend will be available at:
- 🌐 `https://localhost:5001`
- 🌐 `http://localhost:5000`

---

## 🔌 API Endpoints

### 🔑 **Authentication**
- `POST /api/auth/register` – Register a new user.
- `POST /api/auth/login` – Login and retrieve a JWT token.

### 💬 **Messaging**
- `POST /api/messages` – Send a message.
- `GET /api/messages/{userId}` – Retrieve messages for a specific user.

### 🖼️ **Profile**
- `GET /api/users/{userId}` – Get user profile details.
- `PUT /api/users/profile` – Update profile info.

---

## 🛡️ Security
We use **JWT (JSON Web Tokens)** for secure user authentication and authorization. Always keep your secret keys safe! 🔒

---

## 🤝 Contributing
We ❤️ contributions!  
Feel free to fork this repository and submit a pull request with your improvements. Let’s build something amazing together! 🌟

---

## 📜 License
This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for more details.

---

## 📞 Contact
For questions, suggestions, or feedback, feel free to reach out:  
**Ulug'bek Keldibekov**  
📧 Email: [keldibekovulugbek@gmail.com](mailto:keldibekovulugbek@gmail.com)  
📱 Phone: +998932925559  
📲 Telegram: [@keldibekov_ulugbek](https://t.me/keldibekov_ulugbek)  
🌐 GitHub: [keldibekovulugbek](https://github.com/keldibekovulugbek)

---

Enjoy building with **Kabutar Backend**! 🕊️🚀
