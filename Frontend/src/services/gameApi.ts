import axios, { type AxiosResponse } from 'axios';
import type {
  GameState,
  GameStateResponse,
  StartGameResponse,
  StartRoundResponse,
  NextRoundResponse,
  NextQuestionResponse,
  RevealAnswerResponse,
  AwardPointsResponse,
  AddErrorResponse,
  RemoveErrorResponse,
  SetTeamNamesResponse,
  SetTeamNamesRequest,
  SignalRInfo,
  QuestionsResponse
} from '@/types/game';

// Создаем экземпляр Axios с базовой конфигурацией
const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Добавляем перехватчик ответов для обработки ошибок
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export class GameApiService {
  /**
   * Получить текущее состояние игры
   */
  static async getGameState(): Promise<GameState> {
    const response: AxiosResponse<GameStateResponse> = await api.get('/game/state');
    return response.data.gameState;
  }

  /**
   * Начать новую игру
   */
  static async startNewGame(): Promise<StartGameResponse> {
    const response: AxiosResponse<StartGameResponse> = await api.post('/game/start');
    return response.data;
  }

  /**
   * Начать раунд
   */
  static async startRound(): Promise<StartRoundResponse> {
    const response: AxiosResponse<StartRoundResponse> = await api.post('/game/start-round');
    return response.data;
  }

  /**
   * Перейти к следующему раунду
   */
  static async nextRound(): Promise<NextRoundResponse> {
    const response: AxiosResponse<NextRoundResponse> = await api.post('/game/next-round');
    return response.data;
  }

  /**
   * Загрузить следующий вопрос
   */
  static async nextQuestion(): Promise<NextQuestionResponse> {
    const response: AxiosResponse<NextQuestionResponse> = await api.post('/game/next-question');
    return response.data;
  }

  /**
   * Открыть ответ
   */
  static async revealAnswer(answerIndex: number): Promise<RevealAnswerResponse> {
    const response: AxiosResponse<RevealAnswerResponse> = await api.post(`/game/reveal-answer/${answerIndex}`);
    return response.data;
  }

  /**
   * Начислить очки команде
   */
  static async awardPoints(teamId: number, answerIndex: number): Promise<AwardPointsResponse> {
    const response: AxiosResponse<AwardPointsResponse> = await api.post(`/game/award-points/${teamId}/${answerIndex}`);
    return response.data;
  }

  /**
   * Добавить ошибку команде
   */
  static async addError(teamId: number): Promise<AddErrorResponse> {
    const response: AxiosResponse<AddErrorResponse> = await api.post(`/game/add-error/${teamId}`);
    return response.data;
  }

  /**
   * Убрать ошибку у команды
   */
  static async removeError(teamId: number): Promise<RemoveErrorResponse> {
    const response: AxiosResponse<RemoveErrorResponse> = await api.post(`/game/remove-error/${teamId}`);
    return response.data;
  }

  /**
   * Установить названия команд
   */
  static async setTeamNames(request: SetTeamNamesRequest): Promise<SetTeamNamesResponse> {
    const response: AxiosResponse<SetTeamNamesResponse> = await api.post('/game/set-team-names', request);
    return response.data;
  }

  /**
   * Получить информацию о SignalR Hub
   */
  static async getSignalRInfo(): Promise<SignalRInfo> {
    const response: AxiosResponse<SignalRInfo> = await api.get('/game/signalr-info');
    return response.data;
  }

  /**
   * Получить список всех вопросов
   */
  static async getQuestions(): Promise<QuestionsResponse> {
    const response: AxiosResponse<QuestionsResponse> = await api.get('/game/questions');
    return response.data;
  }
}

export default GameApiService;