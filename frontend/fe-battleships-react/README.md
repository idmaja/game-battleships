# Battleships Game - Frontend

This is the frontend client for the Battleships Game, built with **React**. It provides an interactive user interface for players to place ships, attack the computer opponent, and receive real-time game updates.

## 🚀 Tech Stack

- **Framework:** [React](https://react.dev/) (v19)
- **Build Tool:** [Create React App](https://www.google.com/search?q=https://create-react-app.dev/&authuser=1)
- **Styling:** [Tailwind CSS](https://tailwindcss.com/)
- **HTTP Client:** [Axios](https://axios-http.com/)
- **Real-time Communication:** [@microsoft/signalr](https://www.npmjs.com/package/@microsoft/signalr)
- **Drag & Drop:** [@dnd-kit/core](https://dndkit.com/)
- **Icons:** [Phosphor Icons](https://phosphoricons.com/)

## 🛠️ Prerequisites

- [Node.js](https://nodejs.org/) (v18 or higher recommended)
- [npm](https://www.npmjs.com/)

## 📦 Installation

1. Navigate to the project directory
    
```
cd frontend/fe-battleships-react
```
    
2. Install dependencies
    
```
npm install
```
    

## ⚙️ Configuration

Before running the application, ensure the frontend can communicate with the Backend API (running on `.NET`).

### 1. Configure API Base URL

Open `src/services/api.js` and set the `API_BASE_URL` to match your backend address.

**For Local Development:**

JavaScript

```
// src/services/api.js
const API_BASE_URL = 'http://localhost:5069/api/v1/battleships';
```

### 2. Configure SignalR Connection

Open `src/components/MainLayout.jsx` and update the SignalR Hub URL.

**For Local Development:**

JavaScript

```
// src/components/MainLayout.jsx
const newConnection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5069/gameHub')
    .withAutomaticReconnect()
    .build();
```

> Note: If you are running the backend on a different machine or network, replace localhost with the specific IP address (e.g., `http://192.168.1.100:5069`).
> 

## ▶️ Running the App

In the project directory, you can run:

```
npm start
```

Runs the app in the development mode.

Open `http://localhost:3000` to view it in your browser.

The page will reload when you make changes.

You may also see any lint errors in the console.

```
npm run build
```

Builds the app for production to the `build` folder.

It correctly bundles React in production mode and optimizes the build for the best performance.

## 📂 Project Structure

```
src/
├── assets/          # SVG images (ships, logo)
├── components/      # React components
│   ├── Board.jsx          # Individual grid cell component
│   ├── DraggableShip.jsx  # Ship component with drag capabilities
│   ├── GameBoard.jsx      # The main grid board layout
│   ├── GameSetup.jsx      # Initial form to set player name & board size
│   ├── MainLayout.jsx     # Main game logic & state management
│   ├── Modal.jsx          # Popups for game results/notifications
│   └── ShipPlacement.jsx  # Drag & Drop interface for setup phase
├── services/
│   └── api.js       # Axios setup for API calls
├── App.js           # Root component
└── index.js         # Entry point
```

## 🎮 Key Features

1. **Game Setup:** Customizable board size (e.g., 10x10) and ship configurations.
2. **Drag & Drop:** Interactive ship placement using `dnd-kit` with rotation support (Horizontal/Vertical).
3. **Gameplay:**
    - Turn-based attacks against a Computer opponent.
    - Visual indicators for "Hit", "Miss", and "Sunk".
4. **Real-time Updates:** Game logs and results are pushed from the server via SignalR.
5. **Responsive Design:** Fully styled with Tailwind CSS for a modern look.

## 🤝 Contributing

1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/AmazingFeature`).
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

---

## 📄 License

This project is licensed under the **Creative Commons Attribution-NonCommercial 4.0 International Public License**. See the [LICENSE](https://github.com/idmaja/game-battleships?tab=License-1-ov-file#readme) file for details.

---

**Author:** [idmaja](https://github.com/idmaja)
