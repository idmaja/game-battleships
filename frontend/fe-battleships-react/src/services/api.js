import axios from 'axios';

const API_BASE_URL = 'http://localhost:5069/api/v1/battleships'; // private network
// const API_BASE_URL = 'http://172.168.101.88:5069/api/v1/battleships'; // public network

export const initializeGame = (data) => axios.post(`${API_BASE_URL}/initialize-game`, data);
export const getPlayers = () => axios.get(`${API_BASE_URL}/players`);
export const getBoard = (data) => axios.get(`${API_BASE_URL}/board`, { params: data });
export const placeShip = (data) => axios.post(`${API_BASE_URL}/place-ship`, data);
export const removeShip = (data) => axios.post(`${API_BASE_URL}/remove-ship`, data);
export const attack = (data) => axios.post(`${API_BASE_URL}/attack`, data);
export const getScores = () => axios.get(`${API_BASE_URL}/scores`);
export const getPlayerShips = (data) => axios.get(`${API_BASE_URL}/ships`, { params: data });