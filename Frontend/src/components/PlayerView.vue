<template>
  <div class="player-view">
    <!-- Верхняя панель: команды, заголовок игры, команды -->
    <div class="top-panel">
      <!-- Левая команда -->
      <div class="team-left" v-if="gameState.teams[0]">
        <h2 class="team-name">{{ gameState.teams[0].name }}</h2>
        <div class="team-score">{{ gameState.teams[0].score }}</div>
        <div class="team-errors">
          <span 
            v-for="n in 3" 
            :key="n" 
            class="error-mark"
            :class="{ active: n <= gameState.teams[0].errors }"
          >
            ❌
          </span>
        </div>
      </div>

      <!-- Центральный блок с названием игры -->
      <div class="game-center">
        <h1 class="game-title">
          {{ (gameState.roundPoints || 0) * (gameState.roundMultiplier || 1) }}
        </h1>
        <div class="round-info">
          <span class="round">Раунд {{ gameState.currentRound }}</span>
          <span class="multiplier" v-if="gameState.currentMode === 0">
            x{{ gameState.roundMultiplier }}
          </span>
          <span class="rare-mode" v-if="gameState.currentMode === 1">
            РЕДКИЙ ОТВЕТ
          </span>
        </div>
        
        <!-- Вопрос -->
        <div class="question" v-if="gameState.currentQuestion">
          {{ gameState.currentQuestion.text }}
        </div>
      </div>

      <!-- Правая команда -->
      <div class="team-right" v-if="gameState.teams[1]">
        <h2 class="team-name">{{ gameState.teams[1].name }}</h2>
        <div class="team-score">{{ gameState.teams[1].score }}</div>
        <div class="team-errors">
          <span 
            v-for="n in 3" 
            :key="n" 
            class="error-mark"
            :class="{ active: n <= gameState.teams[1].errors }"
          >
            ❌
          </span>
        </div>
      </div>
    </div>

    <!-- Ответы -->
    <div class="answers-section" v-if="gameState.currentQuestion">
      <div class="answers-list">
        <div 
          v-for="(answer, index) in gameState.currentQuestion.answers" 
          :key="index"
          class="answer-card"
          :class="{ revealed: gameState.revealedAnswers.includes(index) }"
        >
          <div class="answer-content">
            <div class="answer-number">{{ index + 1 }}</div>
            <div class="answer-text">{{ answer.text }}</div>
            <div class="answer-points">{{ answer.points }}</div>
          </div>
          <div class="answer-back">
            <div class="answer-number">{{ index + 1 }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Статус игры -->
    <div class="game-status" v-if="!gameState.isGameActive">
      <h2>Игра не началась</h2>
      <p>Ожидание запуска игры...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import type { GameState, Team } from '@/types/game'
import { GameMode } from '@/types/game'
import GameApiService from '@/services/gameApi'
import signalRService from '@/services/signalR'

// Реактивные данные
const gameState = ref<GameState>({
  teams: [],
  currentRound: 1,
  currentQuestion: null,
  roundMultiplier: 1,
  isGameActive: false,
  isRoundActive: false,
  revealedAnswers: [],
  currentMode: GameMode.Normal,
  roundPoints: 0
})

// Переменная для отслеживания предыдущего раунда
const previousRound = ref<number>(1)

// Звуковые эффекты
const successAudio = ref<HTMLAudioElement | null>(null)
const errorAudio = ref<HTMLAudioElement | null>(null)
const newRoundAudio = ref<HTMLAudioElement | null>(null)

// Инициализация звуковых файлов
const initAudioFiles = (): void => {
  try {
    // Звук успешного открытия карточки
    successAudio.value = new Audio('/sounds/success.mp3')
    successAudio.value.preload = 'auto'
    successAudio.value.volume = 0.7
    
    // Обработчик ошибок загрузки для success звука
    successAudio.value.addEventListener('error', (e) => {
      console.warn('⚠️ Не удалось загрузить success.mp3:', e)
      successAudio.value = null
    })
    
    // Звук ошибки/неправильного ответа
    errorAudio.value = new Audio('/sounds/error.mp3')
    errorAudio.value.preload = 'auto'
    errorAudio.value.volume = 0.8
    
    // Обработчик ошибок загрузки для error звука
    errorAudio.value.addEventListener('error', (e) => {
      console.warn('⚠️ Не удалось загрузить error.mp3:', e)
      errorAudio.value = null
    })
    
    // Звук нового раунда
    newRoundAudio.value = new Audio('/sounds/newround.mp3')
    newRoundAudio.value.preload = 'auto'
    newRoundAudio.value.volume = 0.6
    
    // Обработчик ошибок загрузки для newround звука
    newRoundAudio.value.addEventListener('error', (e) => {
      console.warn('⚠️ Не удалось загрузить newround.mp3:', e)
      newRoundAudio.value = null
    })
    
    console.log('🎵 Звуковые файлы инициализированы')
  } catch (error) {
    console.warn('⚠️ Ошибка инициализации звуковых файлов:', error)
  }
}

// Воспроизведение звука успешного открытия карточки
const playSuccessSound = (): void => {
  try {
    if (successAudio.value && successAudio.value.readyState >= 2) {
      successAudio.value.currentTime = 0 // Сброс позиции для повторного воспроизведения
      successAudio.value.play().catch(error => {
        console.warn('Не удалось воспроизвести звук успеха:', error.message)
      })
    } else {
      console.log('🔇 MP3 файл success.mp3 недоступен - игра продолжается без звука')
    }
  } catch (error) {
    console.warn('Ошибка воспроизведения звука успеха:', error)
  }
}

// Воспроизведение звука ошибки
const playErrorSound = (): void => {
  try {
    if (errorAudio.value && errorAudio.value.readyState >= 2) {
      errorAudio.value.currentTime = 0 // Сброс позиции для повторного воспроизведения
      errorAudio.value.play().catch(error => {
        console.warn('Не удалось воспроизвести звук ошибки:', error.message)
      })
    } else {
      console.log('🔇 MP3 файл error.mp3 недоступен - игра продолжается без звука')
    }
  } catch (error) {
    console.warn('Ошибка воспроизведения звука ошибки:', error)
  }
}

// Воспроизведение звука нового раунда
const playNewRoundSound = (): void => {
  try {
    if (newRoundAudio.value && newRoundAudio.value.readyState >= 2) {
      newRoundAudio.value.currentTime = 0 // Сброс позиции для повторного воспроизведения
      newRoundAudio.value.play().catch(error => {
        console.warn('Не удалось воспроизвести звук нового раунда:', error.message)
      })
    } else {
      console.log('🔇 MP3 файл newround.mp3 недоступен - игра продолжается без звука')
    }
  } catch (error) {
    console.warn('Ошибка воспроизведения звука нового раунда:', error)
  }
}

// Методы анимации
const animateAnswerReveal = (answerIndex: number): void => {
  // Проигрываем звук успешного открытия
  playSuccessSound()
  
  const answerCard = document.querySelectorAll('.answer-card')[answerIndex] as HTMLElement
  if (answerCard) {
    answerCard.classList.add('flip-animation')
    setTimeout(() => {
      answerCard.classList.remove('flip-animation')
    }, 800)
  }
}

const animateScoreUpdate = (): void => {
  const scores = document.querySelectorAll('.team-score')
  scores.forEach((score) => {
    const scoreElement = score as HTMLElement
    scoreElement.classList.add('score-update')
    setTimeout(() => {
      scoreElement.classList.remove('score-update')
    }, 1000)
  })
}

// Загрузка начального состояния игры
const loadGameState = async (): Promise<void> => {
  try {
    const state = await GameApiService.getGameState()
    gameState.value = state
    // Инициализируем previousRound текущим значением
    previousRound.value = state.currentRound
    console.log(`🎮 Загружено состояние игры, текущий раунд: ${state.currentRound}`)
  } catch (error) {
    console.error('❌ Ошибка загрузки состояния игры:', error)
  }
}

// Инициализация при монтировании компонента
onMounted(async () => {
  try {
    // Инициализируем звуковые файлы
    initAudioFiles()
    
    // Загружаем текущее состояние
    await loadGameState()
    
    // Подключаемся к SignalR с колбэками
    await signalRService.initialize({
      onGameStateChanged: (newGameState: GameState) => {
        // Проверяем, изменился ли номер раунда
        if (newGameState.currentRound > previousRound.value && newGameState.isGameActive) {
          console.log(`🎵 Новый раунд ${newGameState.currentRound}! Проигрываем мелодию`)
          playNewRoundSound()
          previousRound.value = newGameState.currentRound
        }
        
        gameState.value = newGameState
      },
      onAnswerRevealed: (answerIndex: number) => {
        animateAnswerReveal(answerIndex)
      },
      onScoreUpdated: (teams: Team[]) => {
        gameState.value.teams = teams
        animateScoreUpdate()
      },
      onErrorsUpdated: (teams: Team[]) => {
        gameState.value.teams = teams
        // Проигрываем звук ошибки при добавлении ошибки команде
        playErrorSound()
      }
    })
  } catch (error) {
    console.error('❌ Ошибка инициализации PlayerView:', error)
  }
})

// Очистка при размонтировании
onUnmounted(async () => {
  await signalRService.disconnect()
  
  // Очистка аудио элементов
  if (successAudio.value) {
    successAudio.value.pause()
    successAudio.value = null
  }
  if (errorAudio.value) {
    errorAudio.value.pause()
    errorAudio.value = null
  }
  if (newRoundAudio.value) {
    newRoundAudio.value.pause()
    newRoundAudio.value = null
  }
})
</script>

<style scoped>
.player-view {
  height: 100vh;
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
  color: white;
  padding: 1rem;
  font-family: 'Roboto', sans-serif;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.top-panel {
  height: 30vh;
  max-height: 300px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  backdrop-filter: blur(10px);
}

.team-left, .team-right {
  flex: 1;
  text-align: center;
  max-width: 300px;
}

.game-center {
  flex: 1;
  text-align: center;
  padding: 0 2rem;
}

.game-title {
  font-size: 4rem;
  font-weight: 900;
  color: #ffdd00;
  text-shadow: 3px 3px 6px rgba(0, 0, 0, 0.5);
  margin: 0;
}

.round-info {
  display: flex;
  justify-content: center;
  gap: 0.8rem;
  margin-top: 0.8rem;
  flex-wrap: wrap;
}

.round, .multiplier, .rare-mode {
  font-size: 1rem;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.2);
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  backdrop-filter: blur(10px);
}

.rare-mode {
  background: rgba(255, 107, 107, 0.8);
  animation: pulse 2s infinite;
}

.team-name {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 0.8rem;
  color: #ffdd00;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
}

.team-score {
  font-size: 6rem;
  font-weight: 900;
  color: white;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.team-errors {
  display: flex;
  justify-content: center;
  gap: 0.3rem;
  margin-top: 0.8rem;
}

.error-mark {
  font-size: 3rem;
  opacity: 0.3;
  transition: all 0.3s ease;
}

.error-mark.active {
  opacity: 1;
  animation: errorPulse 1s ease-in-out;
}

.question-section {
  text-align: center;
  margin-bottom: 3rem;
}

.game-center .question {
  font-size: 2.5rem;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.15);
  padding: 1rem;
  border-radius: 8px;
  backdrop-filter: blur(15px);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  margin-top: 2rem;
  max-width: 100%;
}

.question {
  font-size: 2.5rem;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.15);
  padding: 2rem;
  border-radius: 20px;
  backdrop-filter: blur(15px);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  max-width: 80%;
  margin: 0 auto;
}

.answers-section {
  flex: 1;
  width: 100%;
  max-width: none;
  margin: 0;
  padding: 0 1rem;
  height: calc(70vh - 2rem);
  overflow-y: auto;
}

.answers-list {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  padding: 0.5rem;
}

.answer-card {
  position: relative;
  height: 80px;
  perspective: 1000px;
  cursor: pointer;
}

.answer-content,
.answer-back {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  font-weight: 700;
  backface-visibility: hidden;
  transition: transform 0.6s ease;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
}

.answer-back {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  transform: rotateX(0deg);
  justify-content: center;
}

.answer-content {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
  transform: rotateX(-180deg);
}

.answer-card.revealed .answer-back {
  transform: rotateX(180deg);
}

.answer-card.revealed .answer-content {
  transform: rotateX(0deg);
}

.answer-number {
  font-size: 2rem;
  font-weight: 900;
  color: #ffdd00;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
  min-width: 40px;
}

.answer-text {
  font-size: 3rem;
  flex-grow: 1;
  text-align: center;
  margin: 0 0.8rem;
}

.answer-points {
  font-size: 3rem;
  font-weight: 900;
  color: white;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
  min-width: 50px;
  text-align: right;
}

.game-status {
  text-align: center;
  margin-top: 4rem;
}

.game-status h2 {
  font-size: 3rem;
  color: #ffdd00;
  margin-bottom: 1rem;
}

.game-status p {
  font-size: 1.5rem;
  opacity: 0.8;
}

/* Анимации */
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

@keyframes errorPulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.2); }
  100% { transform: scale(1); }
}

.flip-animation {
  animation: flipReveal 0.8s ease-in-out;
}

@keyframes flipReveal {
  0% { transform: rotateX(0deg) scale(1); }
  50% { transform: rotateX(90deg) scale(1.05); }
  100% { transform: rotateX(180deg) scale(1); }
}

.score-update {
  animation: scoreGlow 1s ease-in-out;
}

@keyframes scoreGlow {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); filter: brightness(1.5); }
}
</style>

/* 
/* Адаптивность */
@media (max-width: 768px) {
  .player-view {
    padding: 0.5rem;
  }
  
  .top-panel {
    height: 35vh;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }
  
  .game-center {
    order: -1;
    padding: 0;
  }
  
  .game-title {
    font-size: 2rem;
  }
  
  .round-info {
    gap: 0.4rem;
  }
  
  .round, .multiplier, .rare-mode {
    font-size: 0.8rem;
    padding: 0.2rem 0.6rem;
  }
  
  .team-left, .team-right {
    max-width: 100%;
  }
  
  .team-name {
    font-size: 1.2rem;
  }
  
  .team-score {
    font-size: 2rem;
  }
  
  .game-center .question {
    font-size: 1rem;
    padding: 0.6rem;
    margin-top: 0.6rem;
  }
  
  .answers-section {
    height: calc(65vh - 1rem);
  }
  
  .answers-list {
    gap: 0.3rem;
    padding: 0.3rem;
  }
  
  .answer-card {
    height: 50px;
  }
  
  .answer-content,
  .answer-back {
    padding: 0.6rem 0.8rem;
  }
  
  .answer-number {
    font-size: 0.9rem;
  }
  
  .answer-text {
    font-size: 0.9rem;
  }
  
  .answer-points {
    font-size: 0.9rem;
    color: white;
  }
}

/* Очень маленькие экраны */
@media (max-width: 480px) {
  .player-view {
    padding: 0.3rem;
  }
  
  .top-panel {
    height: 40vh;
    padding: 0.8rem;
  }
  
  .game-title {
    font-size: 1.5rem;
  }
  
  .game-center .question {
    font-size: 0.9rem;
    padding: 0.5rem;
    margin-top: 0.5rem;
  }
  
  .answers-section {
    height: calc(60vh - 1rem);
  }
  
  .answers-list {
    gap: 0.2rem;
    padding: 0.2rem;
  }
  
  .answer-card {
    height: 45px;
  }
  
  .answer-content,
  .answer-back {
    padding: 0.4rem 0.6rem;
  }
  
  .answer-number {
    font-size: 0.8rem;
  }
  
  .answer-text {
    font-size: 0.8rem;
  }
  
  .answer-points {
    font-size: 0.8rem;
    color: white;
  }
  
  .team-name {
    font-size: 1rem;
    font-size: 1.2rem;
  }
  
  .team-score {
    font-size: 2rem;
  }
  
  .error-mark {
    font-size: 1.5rem;
  }
}

/* Большие экраны */
@media (min-width: 1400px) {
  .top-panel {
    padding: 3rem;
  }
  
  .game-title {
    font-size: 3rem;
  }
  
  .game-center .question {
    font-size: 1.5rem;
    padding: 1rem;
    margin-top: 1rem;
  }
  
  .answers-section {
    padding: 0 2rem;
  }
  
  .answers-list {
    gap: 1rem;
  }
  
  .answer-card {
    height: 80px;
  }
  
  .answer-content,
  .answer-back {
    padding: 1rem 1.5rem;
  }
  
  .answer-number {
    font-size: 1.5rem;
  }
  
  .answer-text {
    font-size: 1.3rem;
  }
  
  .answer-points {
    font-size: 1.5rem;
    color: white;
  }
  
  .team-name {
    font-size: 2rem;
  }
  
  .team-score {
    font-size: 3rem;
  }
} */