import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import type { GameState, Team } from '@/types/game';

export interface SignalRCallbacks {
  onGameStateChanged?: (gameState: GameState) => void;
  onAnswerRevealed?: (answerIndex: number) => void;
  onScoreUpdated?: (teams: Team[]) => void;
  onErrorsUpdated?: (teams: Team[]) => void;
  onRoundStarted?: () => void;
  onGameReset?: () => void;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private callbacks: SignalRCallbacks = {};

  /**
   * Инициализация соединения с SignalR Hub
   */
  async initialize(callbacks: SignalRCallbacks = {}): Promise<void> {
    this.callbacks = callbacks;

    this.connection = new HubConnectionBuilder()
      .withUrl('/gamehub')
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Регистрируем обработчики событий
    this.setupEventHandlers();

    try {
      await this.connection.start();
      console.log('✅ SignalR подключен');
      
      // Присоединяемся к игре
      await this.connection.invoke('JoinGame');
    } catch (error) {
      console.error('❌ Ошибка подключения к SignalR:', error);
      throw error;
    }
  }

  /**
   * Настройка обработчиков событий
   */
  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Изменение состояния игры
    this.connection.on('GameStateChanged', (gameState: GameState) => {
      console.log('🎯 Состояние игры изменено:', gameState);
      this.callbacks.onGameStateChanged?.(gameState);
    });

    // Открытие ответа
    this.connection.on('AnswerRevealed', (answerIndex: number) => {
      console.log('💡 Открыт ответ:', answerIndex);
      this.callbacks.onAnswerRevealed?.(answerIndex);
    });

    // Обновление счета
    this.connection.on('ScoreUpdated', (teams: Team[]) => {
      console.log('📊 Счет обновлен:', teams);
      this.callbacks.onScoreUpdated?.(teams);
    });

    // Обновление ошибок
    this.connection.on('ErrorsUpdated', (teams: Team[]) => {
      console.log('❌ Ошибки обновлены:', teams);
      this.callbacks.onErrorsUpdated?.(teams);
    });

    // Старт раунда
    this.connection.on('RoundStarted', () => {
      console.log('🚀 Раунд начался');
      this.callbacks.onRoundStarted?.();
    });

    // Сброс игры
    this.connection.on('GameReset', () => {
      console.log('🔄 Игра сброшена');
      this.callbacks.onGameReset?.();
    });

    // Обработка переподключения
    this.connection.onreconnected(() => {
      console.log('🔄 SignalR переподключен');
    });

    this.connection.onreconnecting(() => {
      console.log('⏳ SignalR переподключается...');
    });

    this.connection.onclose((error) => {
      console.log('🔌 SignalR соединение закрыто:', error);
    });
  }

  /**
   * Обновить колбэки
   */
  updateCallbacks(callbacks: SignalRCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks };
  }

  /**
   * Отключение от SignalR Hub
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('👋 SignalR отключен');
      } catch (error) {
        console.error('❌ Ошибка при отключении SignalR:', error);
      }
    }
  }

  /**
   * Проверка состояния соединения
   */
  get isConnected(): boolean {
    return this.connection?.state === 'Connected';
  }

  /**
   * Получение состояния соединения
   */
  get connectionState(): string {
    return this.connection?.state || 'Disconnected';
  }
}

// Создаем единственный экземпляр сервиса
export const signalRService = new SignalRService();

export default signalRService;