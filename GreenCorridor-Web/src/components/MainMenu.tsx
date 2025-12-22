import { useGameStore, missions } from '../stores/gameStore'
import './MainMenu.css'

export default function MainMenu() {
  const { startMission, stats, setGameStatus } = useGameStore()

  return (
    <div className="main-menu-overlay">
      <div className="main-menu">
        <div className="menu-header">
          <h1>🚑 EMERGENCY AMBULANCE SIMULATOR</h1>
          <p className="menu-subtitle">Green Corridor System</p>
        </div>

        <div className="menu-content">
          <section className="menu-section">
            <h2>Select Mission</h2>
            <div className="missions-grid">
              {missions.map((mission) => (
                <div
                  key={mission.id}
                  className="mission-card"
                  onClick={() => startMission(mission)}
                >
                  <div className="mission-header">
                    <span className={`mission-difficulty difficulty-${mission.difficulty.toLowerCase()}`}>
                      {mission.difficulty}
                    </span>
                    <span className="mission-time">⏱️ {Math.floor(mission.timeLimit / 60)} min</span>
                  </div>
                  <h3>{mission.name}</h3>
                  <p>{mission.description}</p>
                  <div className="mission-details">
                    <span>📍 {mission.trafficDensity} Traffic</span>
                    <span>🌤️ {mission.weather}</span>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="menu-section">
            <h2>Statistics</h2>
            <div className="stats-grid">
              <div className="stat-item">
                <div className="stat-label">Missions Completed</div>
                <div className="stat-value">{stats.missionsCompleted}</div>
              </div>
              <div className="stat-item">
                <div className="stat-label">Total Score</div>
                <div className="stat-value">{stats.totalScore.toLocaleString()}</div>
              </div>
              <div className="stat-item">
                <div className="stat-label">Best Time</div>
                <div className="stat-value">
                  {stats.bestTime > 0 ? `${Math.floor(stats.bestTime / 60)}:${Math.floor(stats.bestTime % 60).toString().padStart(2, '0')}` : 'N/A'}
                </div>
              </div>
              <div className="stat-item">
                <div className="stat-label">Emergency Activations</div>
                <div className="stat-value">{stats.emergencyActivations}</div>
              </div>
            </div>
          </section>
        </div>

        <div className="menu-footer">
          <button className="help-button" onClick={() => setGameStatus('playing')}>
            How to Play
          </button>
        </div>
      </div>
    </div>
  )
}

