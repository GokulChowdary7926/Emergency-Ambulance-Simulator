import { useGameStore } from '../stores/gameStore'
import './GameOver.css'

interface GameOverProps {
  type: 'won' | 'lost'
  reason?: string
}

export default function GameOver({ type, reason }: GameOverProps) {
  const { score, currentMission, gameTime, setGameStatus, startMission } = useGameStore()

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60)
    const secs = Math.floor(seconds % 60)
    return `${mins}:${secs.toString().padStart(2, '0')}`
  }

  return (
    <div className="game-over-overlay">
      <div className="game-over-modal">
        {type === 'won' ? (
          <>
            <div className="game-over-icon success">🎉</div>
            <h1>MISSION SUCCESS!</h1>
            <p className="game-over-message">Patient safely delivered to hospital</p>
          </>
        ) : (
          <>
            <div className="game-over-icon failure">❌</div>
            <h1>MISSION FAILED</h1>
            <p className="game-over-message">{reason || 'Mission incomplete'}</p>
          </>
        )}

        <div className="game-over-stats">
          <div className="stat-row">
            <span>Final Score:</span>
            <span className="stat-value">{score.toLocaleString()}</span>
          </div>
          <div className="stat-row">
            <span>Time Taken:</span>
            <span className="stat-value">{formatTime(gameTime)}</span>
          </div>
          {currentMission && (
            <div className="stat-row">
              <span>Mission:</span>
              <span className="stat-value">{currentMission.name}</span>
            </div>
          )}
        </div>

        <div className="game-over-actions">
          <button
            className="action-button primary"
            onClick={() => {
              if (currentMission) {
                startMission(currentMission)
              }
            }}
          >
            Retry Mission
          </button>
          <button
            className="action-button secondary"
            onClick={() => setGameStatus('menu')}
          >
            Main Menu
          </button>
        </div>
      </div>
    </div>
  )
}


