<template>
  <div class="player-view">
    <!-- Заголовок игры -->
    <div class="game-header">
      <h1 class="game-title">100 К 1</h1>
      <div class="round-info">
        <span class="round">Раунд {{ gameState.currentRound }}</span>
        <span class="multiplier" v-if="gameState.currentMode === 0">
          x{{ gameState.roundMultiplier }}
        </span>
        <span class="rare-mode" v-else>
          РЕДКИЙ ОТВЕТ
        </span>
      </div>
    </div>

    <!-- Счет команд -->
    <div class="teams-score">
      <div class="team" v-for="team in gameState.teams" :key="team.id">
        <h2 class="team-name">{{ team.name }}</h2>
        <div class="team-score">{{ team.score }}</div>
        <div class="team-errors">
          <span 
            v-for="n in 3" 
            :key="n" 
            class="error-mark"
            :class="{ active: n <= team.errors }"
          >
            ❌
          </span>
        </div>
      </div>
    </div>

    <!-- Вопрос -->
    <div class="question-section" v-if="gameState.currentQuestion">
      <div class="question">
        {{ gameState.currentQuestion.text }}
      </div>
    </div>

    <!-- Ответы -->
    <div class="answers-section" v-if="gameState.currentQuestion">
      <div class="answers-grid">
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

<script>
import * as signalR from '@microsoft/signalr'

export default {
  name: 'PlayerView',
  data() {
    return {
      connection: null,
      gameState: {
        teams: [],
        currentRound: 1,
        currentQuestion: null,
        roundMultiplier: 1,
        isGameActive: false,
        isRoundActive: false,
        revealedAnswers: [],
        currentMode: 0
      }
    }
  },
  async mounted() {
    await this.initializeSignalR()
    await this.loadGameState()
  },
  beforeUnmount() {
    if (this.connection) {
      this.connection.stop()
    }
  },
  methods: {
    async initializeSignalR() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('/gamehub')
        .build()

      this.connection.on('GameStateChanged', (gameState) => {
        this.gameState = gameState
      })

      this.connection.on('AnswerRevealed', (answerIndex) => {
        // Анимация открытия ответа
        this.animateAnswerReveal(answerIndex)
      })

      this.connection.on('ScoreUpdated', (teams) => {
        this.gameState.teams = teams
        this.animateScoreUpdate()
      })

      this.connection.on('ErrorsUpdated', (teams) => {
        this.gameState.teams = teams
      })

      try {
        await this.connection.start()
        await this.connection.invoke('JoinGame')
      } catch (err) {
        console.error('SignalR connection error:', err)
      }
    },

    async loadGameState() {
      try {
        const response = await fetch('/api/game/state')
        if (response.ok) {
          this.gameState = await response.json()
        }
      } catch (error) {
        console.error('Error loading game state:', error)
      }
    },

    animateAnswerReveal(answerIndex) {
      const answerCard = document.querySelectorAll('.answer-card')[answerIndex]
      if (answerCard) {
        answerCard.classList.add('flip-animation')
        setTimeout(() => {
          answerCard.classList.remove('flip-animation')
        }, 800)
      }
    },

    animateScoreUpdate() {
      const scores = document.querySelectorAll('.team-score')
      scores.forEach(score => {
        score.classList.add('score-update')
        setTimeout(() => {
          score.classList.remove('score-update')
        }, 1000)
      })
    }
  }
}
</script>

<style scoped>
.player-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
  color: white;
  padding: 2rem;
  font-family: 'Roboto', sans-serif;
}

.game-header {
  text-align: center;
  margin-bottom: 2rem;
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
  gap: 2rem;
  margin-top: 1rem;
}

.round, .multiplier, .rare-mode {
  font-size: 1.5rem;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.2);
  padding: 0.5rem 1rem;
  border-radius: 10px;
  backdrop-filter: blur(10px);
}

.rare-mode {
  background: rgba(255, 107, 107, 0.8);
  animation: pulse 2s infinite;
}

.teams-score {
  display: flex;
  justify-content: space-around;
  margin-bottom: 3rem;
}

.team {
  text-align: center;
}

.team-name {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 1rem;
  color: #ffdd00;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
}

.team-score {
  font-size: 4rem;
  font-weight: 900;
  background: linear-gradient(135deg, #ff6b6b, #ee5a24);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.team-errors {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
}

.error-mark {
  font-size: 2rem;
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
  max-width: 1200px;
  margin: 0 auto;
}

.answers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 1.5rem;
}

.answer-card {
  position: relative;
  height: 120px;
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
  padding: 1.5rem;
  font-weight: 700;
  backface-visibility: hidden;
  transition: transform 0.6s ease;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
}

.answer-back {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  transform: rotateY(0deg);
  justify-content: center;
}

.answer-content {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
  transform: rotateY(-180deg);
}

.answer-card.revealed .answer-back {
  transform: rotateY(180deg);
}

.answer-card.revealed .answer-content {
  transform: rotateY(0deg);
}

.answer-number {
  font-size: 2rem;
  font-weight: 900;
  color: #ffdd00;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
}

.answer-text {
  font-size: 1.5rem;
  flex-grow: 1;
  text-align: center;
  margin: 0 1rem;
}

.answer-points {
  font-size: 2rem;
  font-weight: 900;
  color: #ffdd00;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
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
  0% { transform: rotateY(0deg) scale(1); }
  50% { transform: rotateY(90deg) scale(1.05); }
  100% { transform: rotateY(180deg) scale(1); }
}

.score-update {
  animation: scoreGlow 1s ease-in-out;
}

@keyframes scoreGlow {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); filter: brightness(1.5); }
}

/* Адаптивность */
@media (max-width: 768px) {
  .game-title {
    font-size: 2.5rem;
  }
  
  .team-name {
    font-size: 1.5rem;
  }
  
  .team-score {
    font-size: 2.5rem;
  }
  
  .question {
    font-size: 1.8rem;
    padding: 1.5rem;
  }
  
  .answers-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .answer-content,
  .answer-back {
    padding: 1rem;
  }
  
  .answer-text {
    font-size: 1.2rem;
  }
}
</style>