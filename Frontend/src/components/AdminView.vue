<template>
  <div class="admin-view">
    <div class="admin-header">
      <h1>Админка игры "100 к 1"</h1>
      <div class="header-controls">
        <button @click="goToPlayerView" class="player-view-btn">
          📺 Перейти к экрану игроков
        </button>
      </div>
    </div>

    <!-- Настройка команд -->
    <div class="section">
      <h2>Настройка команд</h2>
      <div class="team-setup">
        <div class="team-input">
          <label>Команда 1:</label>
          <input v-model="team1Name" placeholder="Название команды 1" />
        </div>
        <div class="team-input">
          <label>Команда 2:</label>
          <input v-model="team2Name" placeholder="Название команды 2" />
        </div>
        <button @click="updateTeamNames" class="btn btn-primary">
          Обновить названия команд
        </button>
      </div>
    </div>

    <!-- Управление игрой -->
    <div class="section">
      <h2>Управление игрой</h2>
      <div class="game-controls">
        <button @click="startNewGame" class="btn btn-success">
          🎯 Новая игра
        </button>
        <button @click="nextRound" class="btn btn-info" :disabled="!gameState.isGameActive || gameState.currentRound >= 5">
          ⏭️ Следующий раунд
        </button>
      </div>
    </div>

    <!-- Информация о текущем состоянии -->
    <div class="section">
      <h2>Текущее состояние</h2>
      <div class="game-status">
        <div class="status-item">
          <strong>Раунд:</strong> {{ gameState.currentRound }}
        </div>
        <div class="status-item">
          <strong>Множитель:</strong> x{{ gameState.roundMultiplier }}
        </div>
        <div class="status-item">
          <strong>Режим:</strong> 
          {{ gameState.currentMode === 0 ? 'Обычный' : 'Редкий ответ' }}
        </div>
        <div class="status-item">
          <strong>Очки раунда:</strong> 
          <span class="round-points">{{ gameState.roundPoints || 0 }}</span>
        </div>
        <div class="status-item">
          <strong>Итого с множителем:</strong> 
          <span class="final-points">{{ (gameState.roundPoints || 0) * gameState.roundMultiplier }}</span>
        </div>
        <div class="status-item">
          <strong>Игра активна:</strong> 
          <span :class="gameState.isGameActive ? 'status-active' : 'status-inactive'">
            {{ gameState.isGameActive ? 'Да' : 'Нет' }}
          </span>
        </div>
        <div class="status-item">
          <strong>Раунд активен:</strong> 
          <span :class="gameState.isRoundActive ? 'status-active' : 'status-inactive'">
            {{ gameState.isRoundActive ? 'Да' : 'Нет' }}
          </span>
        </div>
      </div>
    </div>

    <!-- Настройка раунда -->
    <div class="section">
      <h2>Настройка раунда</h2>
      <div class="round-setup">
        <div class="round-control">
          <label for="multiplier">Множитель очков:</label>
          <select v-model="selectedMultiplier" id="multiplier" class="form-control">
            <option value="1">x1 (Обычный)</option>
            <option value="2">x2 (Двойной)</option>
            <option value="3">x3 (Тройной)</option>
            <option value="5">x5 (Пятикратный)</option>
          </select>
        </div>
        
        <div class="round-control">
          <label for="gameMode">Режим раунда:</label>
          <select v-model="selectedMode" id="gameMode" class="form-control">
            <option value="0">Обычный раунд</option>
            <option value="1">Самый редкий ответ</option>
          </select>
        </div>
        
        <button @click="updateRoundSettings" class="btn btn-primary" :disabled="!gameState.isGameActive">
          🔧 Применить настройки раунда
        </button>
      </div>
    </div>

    <!-- Управление командами -->
    <div class="section">
      <h2>Счет команд</h2>
      <div class="teams-management">
        <div v-for="team in gameState.teams" :key="team.id" class="team-card">
          <h3>{{ team.name }}</h3>
          <div class="team-score">Очки: {{ team.score }}</div>
          <div class="team-errors">Ошибки: {{ team.errors }} / 3</div>
          
          <!-- Блок изменения очков -->
          <div class="score-controls">
            <label :for="'newScore' + team.id">Новые очки:</label>
            <input 
              :id="'newScore' + team.id"
              type="number" 
              :value="team.id === 1 ? team1NewScore : team2NewScore"
              @input="team.id === 1 ? team1NewScore = $event.target.value : team2NewScore = $event.target.value"
              min="0"
              class="score-input"
            />
            <button 
              @click="setTeamScore(team.id, team.id === 1 ? team1NewScore : team2NewScore)" 
              class="btn btn-primary btn-sm"
            >
              ✏️ Установить
            </button>
          </div>
          
          <div class="team-controls">
            <button @click="addError(team.id)" class="btn btn-danger btn-sm" :disabled="team.errors >= 3">
              ❌ Добавить ошибку
            </button>
            <button @click="removeError(team.id)" class="btn btn-warning btn-sm" :disabled="team.errors <= 0">
              ✅ Убрать ошибку
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Текущий вопрос и ответы -->
    <div class="section" v-if="gameState.currentQuestion">
      <h2>Текущий вопрос</h2>
      <div class="question-section">
        <div class="current-question">
          {{ gameState.currentQuestion.text }}
        </div>
        
        <div class="answers-management">
          <h3>Ответы</h3>
          <div class="answers-grid">
            <div 
              v-for="(answer, index) in gameState.currentQuestion.answers" 
              :key="index"
              class="answer-item"
              :class="{ revealed: gameState.revealedAnswers.includes(index) }"
            >
              <div class="answer-info">
                <span class="answer-number">{{ index + 1 }}</span>
                <span class="answer-text">{{ answer.text }}</span>
                <span class="answer-points">{{ answer.points }} очков</span>
              </div>
              
              <div class="answer-controls">
                <button 
                  @click="revealAnswer(index)" 
                  class="btn btn-info btn-sm"
                  :disabled="gameState.revealedAnswers.includes(index)"
                >
                  👁️ Открыть
                </button>
                <button 
                  @click="revealAnswerWithoutPoints(index)" 
                  class="btn btn-secondary btn-sm"
                  :disabled="gameState.revealedAnswers.includes(index)"
                >
                  👁️ Без очков
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Кнопки присвоения очков команде -->
        <div class="round-award-section" v-if="gameState.roundPoints > 0">
          <h3>Присвоить очки раунда команде</h3>
          <div class="award-buttons">
            <button 
              @click="awardRoundPoints(1)" 
              class="btn btn-success"
            >
              🏆 {{ gameState.teams[0]?.name || 'Команда 1' }} (+{{ (gameState.roundPoints || 0) * gameState.roundMultiplier }})
            </button>
            
            <button 
              @click="awardRoundPoints(2)" 
              class="btn btn-success"
            >
              🏆 {{ gameState.teams[1]?.name || 'Команда 2' }} (+{{ (gameState.roundPoints || 0) * gameState.roundMultiplier }})
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Отсутствие вопроса -->
    <div class="section" v-else>
      <h2>Вопрос не загружен</h2>
      <p>Начните новую игру, чтобы загрузить вопрос</p>
    </div>
  </div>
</template>

<script>
import * as signalR from '@microsoft/signalr'

export default {
  name: 'AdminView',
  data() {
    return {
      connection: null,
      team1Name: 'Команда 1',
      team2Name: 'Команда 2',
      selectedMultiplier: 1,  // Выбранный множитель
      selectedMode: 0,        // Выбранный режим игры
      team1NewScore: 0,       // Новые очки для команды 1
      team2NewScore: 0,       // Новые очки для команды 2
      gameState: {
        teams: [],
        currentRound: 1,
        currentQuestion: null,
        roundMultiplier: 1,
        isGameActive: false,
        isRoundActive: false,
        revealedAnswers: [],
        currentMode: 0,
        roundPoints: 0
      }
    }
  },
  async mounted() {
    console.log('AdminView mounted, initializing...') // Для отладки
    await this.initializeSignalR()
    
    // Небольшая задержка для установления SignalR соединения
    await new Promise(resolve => setTimeout(resolve, 100))
    
    await this.loadGameState()
    console.log('AdminView initialized, team names:', this.team1Name, this.team2Name) // Для отладки
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
        console.log('SignalR GameStateChanged:', gameState) // Для отладки
        
        if (gameState && typeof gameState === 'object') {
          this.gameState = gameState
          
          // Синхронизируем названия команд при обновлении состояния
          if (gameState.teams && gameState.teams.length >= 2) {
            const newTeam1Name = gameState.teams[0].name || 'Команда 1'
            const newTeam2Name = gameState.teams[1].name || 'Команда 2'
            
            // Обновляем только если названия действительно изменились
            if (this.team1Name !== newTeam1Name || this.team2Name !== newTeam2Name) {
              this.team1Name = newTeam1Name
              this.team2Name = newTeam2Name
              console.log('SignalR updated team names:', this.team1Name, this.team2Name) // Для отладки
            }
          }
          
          // Синхронизируем множитель и режим
          this.selectedMultiplier = gameState.roundMultiplier || 1
          this.selectedMode = gameState.currentMode || 0
          
          // Синхронизируем поля ввода очков с текущими очками команд
          if (gameState.teams && gameState.teams.length >= 2) {
            this.team1NewScore = gameState.teams[0].score || 0
            this.team2NewScore = gameState.teams[1].score || 0
          }
        } else {
          console.warn('Invalid gameState received from SignalR:', gameState)
        }
      })

      try {
        await this.connection.start()
        console.log('SignalR connected successfully') // Для отладки
        await this.connection.invoke('JoinGame')
        console.log('Joined SignalR game hub') // Для отладки
      } catch (err) {
        console.error('SignalR connection error:', err)
      }
    },

    async loadGameState() {
      try {
        const response = await fetch('/api/game/state')
        if (response.ok) {
          const data = await response.json()
          console.log('Loaded game state data:', data) // Для отладки
          
          // API возвращает GameStateResponse с полем gameState
          // Проверяем оба варианта - data.gameState и data напрямую
          if (data.gameState) {
            this.gameState = data.gameState
          } else if (data.teams) {
            // Если data содержит teams напрямую, значит это GameState
            this.gameState = data
          } else {
            console.warn('Unexpected API response format:', data)
            return
          }
          
          // Синхронизируем названия команд с загруженным состоянием
          if (this.gameState.teams && this.gameState.teams.length >= 2) {
            const newTeam1Name = this.gameState.teams[0].name || 'Команда 1'
            const newTeam2Name = this.gameState.teams[1].name || 'Команда 2'
            
            // Обновляем только если названия действительно изменились
            if (this.team1Name !== newTeam1Name || this.team2Name !== newTeam2Name) {
              this.team1Name = newTeam1Name
              this.team2Name = newTeam2Name
              console.log('Updated team names:', this.team1Name, this.team2Name) // Для отладки
            }
          } else {
            console.warn('Teams not found in game state:', this.gameState)
          }
          
          // Синхронизируем множитель и режим с состоянием игры
          this.selectedMultiplier = this.gameState.roundMultiplier || 1
          this.selectedMode = this.gameState.currentMode || 0
          console.log('Synced round settings - Multiplier:', this.selectedMultiplier, 'Mode:', this.selectedMode)
          
          // Синхронизируем поля ввода очков с текущими очками команд
          if (this.gameState.teams && this.gameState.teams.length >= 2) {
            this.team1NewScore = this.gameState.teams[0].score || 0
            this.team2NewScore = this.gameState.teams[1].score || 0
          }
        } else {
          console.error('Failed to load game state, status:', response.status)
        }
      } catch (error) {
        console.error('Error loading game state:', error)
      }
    },

    async updateTeamNames() {
      try {
        console.log('Updating team names:', this.team1Name, this.team2Name) // Для отладки
        
        const response = await fetch('/api/game/set-team-names', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            Team1Name: this.team1Name,  // Pascal case для соответствия модели
            Team2Name: this.team2Name   // Pascal case для соответствия модели
          })
        })
        
        if (response.ok) {
          const result = await response.json()
          console.log('Team names update response:', result) // Для отладки
          this.showSuccess('Названия команд обновлены')
        } else {
          console.error('Failed to update team names, status:', response.status)
          const errorText = await response.text()
          console.error('Error response:', errorText)
          this.showError('Ошибка при обновлении названий команд')
        }
      } catch (error) {
        console.error('Error updating team names:', error)
        this.showError('Ошибка при обновлении названий команд')
      }
    },

    async updateRoundSettings() {
      try {
        console.log('Updating round settings - Multiplier:', this.selectedMultiplier, 'Mode:', this.selectedMode)
        
        const response = await fetch('/api/game/set-round-settings', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            Multiplier: parseInt(this.selectedMultiplier),
            Mode: parseInt(this.selectedMode)
          })
        })
        
        if (response.ok) {
          const result = await response.json()
          console.log('Round settings update response:', result)
          this.showSuccess(`Настройки раунда обновлены: x${this.selectedMultiplier}, ${this.selectedMode === 0 ? 'Обычный' : 'Редкий ответ'}`)
        } else {
          console.error('Failed to update round settings, status:', response.status)
          const errorText = await response.text()
          console.error('Error response:', errorText)
          this.showError('Ошибка при обновлении настроек раунда')
        }
      } catch (error) {
        console.error('Error updating round settings:', error)
        this.showError('Ошибка при обновлении настроек раунда')
      }
    },

    async startNewGame() {
      try {
        const response = await fetch('/api/game/start', { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Новая игра началась')
        }
      } catch (error) {
        console.error('Error starting new game:', error)
        this.showError('Ошибка при запуске новой игры')
      }
    },

    async nextRound() {
      try {
        const response = await fetch('/api/game/next-round', { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Переход к следующему раунду')
        }
      } catch (error) {
        console.error('Error going to next round:', error)
        this.showError('Ошибка при переходе к следующему раунду')
      }
    },

    async revealAnswer(answerIndex) {
      try {
        const response = await fetch(`/api/game/reveal-answer/${answerIndex}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Ответ открыт')
        }
      } catch (error) {
        console.error('Error revealing answer:', error)
        this.showError('Ошибка при открытии ответа')
      }
    },

    async revealAnswerWithoutPoints(answerIndex) {
      try {
        const response = await fetch(`/api/game/reveal-answer-no-points/${answerIndex}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Ответ открыт без очков')
        }
      } catch (error) {
        console.error('Error revealing answer without points:', error)
        this.showError('Ошибка при открытии ответа без очков')
      }
    },

    async awardRoundPoints(teamId) {
      try {
        const response = await fetch(`/api/game/award-round-points/${teamId}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Очки раунда присвоены команде')
        }
      } catch (error) {
        console.error('Error awarding round points:', error)
        this.showError('Ошибка при присвоении очков раунда')
      }
    },

    async addError(teamId) {
      try {
        const response = await fetch(`/api/game/add-error/${teamId}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Ошибка добавлена')
        }
      } catch (error) {
        console.error('Error adding error:', error)
        this.showError('Ошибка при добавлении ошибки')
      }
    },

    async removeError(teamId) {
      try {
        const response = await fetch(`/api/game/remove-error/${teamId}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess('Ошибка убрана')
        }
      } catch (error) {
        console.error('Error removing error:', error)
        this.showError('Ошибка при удалении ошибки')
      }
    },

    async setTeamScore(teamId, newScore) {
      try {
        const score = parseInt(newScore)
        if (isNaN(score) || score < 0) {
          this.showError('Введите корректное количество очков (0 или больше)')
          return
        }
        
        const response = await fetch(`/api/game/set-team-score/${teamId}/${score}`, { method: 'POST' })
        if (response.ok) {
          this.showSuccess(`Очки команды ${teamId} установлены на ${score}`)
        }
      } catch (error) {
        console.error('Error setting team score:', error)
        this.showError('Ошибка при установке очков команды')
      }
    },

    goToPlayerView() {
      window.open('#player', '_blank')
    },

    showSuccess(message) {
      // Простая система уведомлений
      const notification = document.createElement('div')
      notification.className = 'notification success'
      notification.textContent = message
      document.body.appendChild(notification)
      setTimeout(() => {
        document.body.removeChild(notification)
      }, 3000)
    },

    showError(message) {
      const notification = document.createElement('div')
      notification.className = 'notification error'
      notification.textContent = message
      document.body.appendChild(notification)
      setTimeout(() => {
        document.body.removeChild(notification)
      }, 3000)
    }
  }
}
</script>

<style scoped>
.admin-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
  font-family: 'Roboto', sans-serif;
  color: white;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 3rem;
  background: rgba(255, 255, 255, 0.1);
  padding: 1.5rem;
  border-radius: 15px;
  backdrop-filter: blur(10px);
}

.admin-header h1 {
  font-size: 2.5rem;
  font-weight: 700;
  color: #ffdd00;
  margin: 0;
}

.player-view-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #ff6b6b, #ee5a24);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.player-view-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.3);
}

.section {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 2rem;
  margin-bottom: 2rem;
  backdrop-filter: blur(10px);
}

.section h2 {
  color: #ffdd00;
  margin-bottom: 1.5rem;
  font-size: 1.8rem;
}

.team-setup {
  display: flex;
  gap: 1rem;
  align-items: end;
  flex-wrap: wrap;
}

.team-input {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.team-input label {
  font-weight: 600;
}

.team-input input {
  padding: 0.75rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-size: 1rem;
}

.team-input input::placeholder {
  color: rgba(255, 255, 255, 0.6);
}

.round-setup {
  display: flex;
  gap: 1.5rem;
  align-items: end;
  flex-wrap: wrap;
}

.round-control {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  min-width: 200px;
}

.round-control label {
  font-weight: 600;
  color: #ffdd00;
}

.form-control {
  padding: 0.75rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-size: 1rem;
}

.form-control option {
  background: #333;
  color: white;
}

.game-controls {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.9rem;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
}

.btn-primary { background: #4834d4; color: white; }
.btn-success { background: #00d2d3; color: white; }
.btn-warning { background: #ff9ff3; color: white; }
.btn-info { background: #54a0ff; color: white; }
.btn-secondary { background: #5f27cd; color: white; }
.btn-danger { background: #ff3838; color: white; }
.btn-sm { padding: 0.5rem 1rem; font-size: 0.8rem; }

.game-status {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.status-item {
  background: rgba(255, 255, 255, 0.1);
  padding: 1rem;
  border-radius: 10px;
  text-align: center;
}

.status-active { color: #00d2d3; font-weight: 700; }
.status-inactive { color: #ff3838; font-weight: 700; }

.teams-management {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
}

.team-card {
  background: rgba(255, 255, 255, 0.1);
  padding: 1.5rem;
  border-radius: 12px;
  text-align: center;
}

.team-card h3 {
  color: #ffdd00;
  margin-bottom: 1rem;
  font-size: 1.5rem;
}

.team-score, .team-errors {
  margin-bottom: 0.5rem;
  font-size: 1.1rem;
}

.team-controls {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
  margin-top: 1rem;
}

.score-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  margin: 0.8rem 0;
  padding: 0.8rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
}

.score-controls label {
  font-weight: 600;
  color: #e0e0e0;
  min-width: fit-content;
}

.score-input {
  width: 80px;
  padding: 0.4rem;
  border: 1px solid #555;
  border-radius: 4px;
  background: #2a2a2a;
  color: white;
  text-align: center;
  font-weight: 600;
}

.current-question {
  background: rgba(255, 255, 255, 0.15);
  padding: 1.5rem;
  border-radius: 12px;
  font-size: 1.3rem;
  font-weight: 600;
  text-align: center;
  margin-bottom: 2rem;
}

.answers-management h3 {
  margin-bottom: 1rem;
  color: #ffdd00;
}

.answers-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.answer-item {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: all 0.3s ease;
}

.answer-item.revealed {
  background: rgba(0, 210, 211, 0.3);
  border-left: 4px solid #00d2d3;
}

.answer-info {
  display: flex;
  gap: 1rem;
  align-items: center;
  flex-grow: 1;
}

.answer-number {
  background: #ffdd00;
  color: #333;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

.answer-text {
  flex-grow: 1;
  font-weight: 600;
}

.answer-points {
  background: rgba(255, 255, 255, 0.2);
  padding: 0.3rem 0.8rem;
  border-radius: 15px;
  font-weight: 600;
  color: #ffdd00;
}

.answer-controls {
  display: flex;
  gap: 0.5rem;
}

/* Стили для счетчика очков раунда */
.round-points {
  font-weight: bold;
  color: #ff6b35;
  font-size: 1.1em;
}

.final-points {
  font-weight: bold;
  color: #4caf50;
  font-size: 1.2em;
}

/* Стили для секции присвоения очков */
.round-award-section {
  margin-top: 2rem;
  padding: 1.5rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  text-align: center;
}

.round-award-section h3 {
  color: #ffdd00;
  margin-bottom: 1rem;
  font-size: 1.3rem;
}

.award-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.award-buttons .btn {
  font-size: 1.1rem;
  padding: 0.8rem 1.5rem;
  min-width: 200px;
}

/* Система уведомлений */
:global(.notification) {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  z-index: 1000;
  animation: slideIn 0.3s ease;
}

:global(.notification.success) {
  background: #00d2d3;
}

:global(.notification.error) {
  background: #ff3838;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

/* Адаптивность */
@media (max-width: 768px) {
  .admin-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .admin-header h1 {
    font-size: 2rem;
  }
  
  .team-setup {
    flex-direction: column;
    align-items: stretch;
  }
  
  .game-controls {
    flex-direction: column;
  }
  
  .answer-item {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .answer-info {
    justify-content: center;
  }
  
  .answer-controls {
    justify-content: center;
    flex-wrap: wrap;
  }
}
</style>