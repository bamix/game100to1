// Типы для игровых моделей
export interface Team {
  id: number;
  name: string;
  score: number;
  errors: number;
}

export interface Answer {
  text: string;
  points: number;
}

export interface Question {
  text: string;
  answers: Answer[];
}

export enum GameMode {
  Normal = 0,
  RareAnswer = 1
}

export interface GameState {
  teams: Team[];
  currentRound: number;
  currentQuestion: Question | null;
  roundMultiplier: number;
  isGameActive: boolean;
  isRoundActive: boolean;
  revealedAnswers: number[];
  currentMode: GameMode;
}

// API Response типы
export interface ApiResponse {
  message: string;
}

export interface GameStateResponse extends ApiResponse {
  gameState: GameState;
}

export interface StartGameResponse extends ApiResponse {}
export interface StartRoundResponse extends ApiResponse {}
export interface NextRoundResponse extends ApiResponse {}
export interface NextQuestionResponse extends ApiResponse {}

export interface RevealAnswerResponse extends ApiResponse {
  answerIndex: number;
}

export interface AwardPointsResponse extends ApiResponse {
  teamId: number;
  answerIndex: number;
}

export interface AddErrorResponse extends ApiResponse {
  teamId: number;
}

export interface RemoveErrorResponse extends ApiResponse {
  teamId: number;
}

export interface SetTeamNamesResponse extends ApiResponse {
  team1: string;
  team2: string;
}

export interface SetTeamNamesRequest {
  team1Name: string;
  team2Name: string;
}

// SignalR типы
export interface SignalREvent {
  name: string;
  description: string;
}

export interface SignalRMethod {
  name: string;
  description: string;
}

export interface SignalRInfo {
  hubUrl: string;
  events: SignalREvent[];
  methods: SignalRMethod[];
}

export interface QuestionSummary {
  id: number;
  text: string;
  answersCount: number;
  totalPoints: number;
}

export interface QuestionsResponse {
  totalCount: number;
  questions: QuestionSummary[];
}