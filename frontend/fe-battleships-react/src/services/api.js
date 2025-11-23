import axios from 'axios';

const API_BASE_URL = 'http://localhost:5069/api/v1/battleships';

export const initializeGame = (data) => axios.post(`${API_BASE_URL}/initialize-game`, data);
export const getPlayers = () => axios.get(`${API_BASE_URL}/players`);
export const getBoard = (playerName) => axios.get(`${API_BASE_URL}/board/${playerName}`);
export const placeShip = (data) => axios.post(`${API_BASE_URL}/place-ship`, data);
export const removeShip = (data) => axios.post(`${API_BASE_URL}/remove-ship`, data);
export const attack = (data) => axios.post(`${API_BASE_URL}/attack`, data);
export const getScores = () => axios.get(`${API_BASE_URL}/scores`);
