<template>
  <div id="app">
    <component v-if="currentRoute" :is="routerView" />
    <div v-else class="route-selector">
      <h1>Игра "100 к 1"</h1>
      <div class="route-buttons">
        <button @click="goToPlayer" class="route-btn player-btn">
          📺 Экран игроков
        </button>
        <button @click="goToAdmin" class="route-btn admin-btn">
          ⚙️ Админка
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import PlayerView from './components/PlayerView.vue'
import AdminView from './components/AdminView.vue'

export default {
  name: 'App',
  components: {
    PlayerView,
    AdminView
  },
  data() {
    return {
      currentRoute: null
    }
  },
  mounted() {
    // Простая маршрутизация на основе hash
    this.updateRoute()
    window.addEventListener('hashchange', this.updateRoute)
  },
  methods: {
    updateRoute() {
      const hash = window.location.hash.slice(1)
      if (hash === 'player' || hash === 'admin') {
        this.currentRoute = hash
      } else {
        this.currentRoute = null
      }
    },
    goToPlayer() {
      window.location.hash = 'player'
    },
    goToAdmin() {
      window.location.hash = 'admin'
    }
  },
  computed: {
    routerView() {
      if (this.currentRoute === 'player') {
        return PlayerView
      } else if (this.currentRoute === 'admin') {
        return AdminView
      }
      return null
    }
  }
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Roboto', sans-serif;
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
  min-height: 100vh;
}

#app {
  min-height: 100vh;
}

.route-selector {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  text-align: center;
}

.route-selector h1 {
  color: #ffdd00;
  font-size: 4rem;
  font-weight: 900;
  text-shadow: 3px 3px 6px rgba(0, 0, 0, 0.5);
  margin-bottom: 3rem;
}

.route-buttons {
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
  justify-content: center;
}

.route-btn {
  padding: 1.5rem 3rem;
  font-size: 1.5rem;
  font-weight: 700;
  border: none;
  border-radius: 15px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.3);
}

.player-btn {
  background: linear-gradient(135deg, #ff6b6b, #ee5a24);
  color: white;
}

.admin-btn {
  background: linear-gradient(135deg, #4834d4, #686de0);
  color: white;
}

.route-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.4);
}

.route-btn:active {
  transform: translateY(-1px);
}
</style>