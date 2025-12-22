import { useGameStore } from '../stores/gameStore'
import './TrafficControlPanel.css'

export default function TrafficControlPanel() {
  const { isEmergencyActive, trafficSignals } = useGameStore()
  
  const preemptedCount = trafficSignals.filter(s => s.preempted).length
  const totalSignals = trafficSignals.length || 20

  return (
    <div className="traffic-control-panel">
      <div className="traffic-header">
        <div className="traffic-title">🚦 TRAFFIC CONTROL</div>
        <div className={`traffic-status ${isEmergencyActive ? 'active' : ''}`}>
          {isEmergencyActive ? 'GREEN CORRIDOR' : 'NORMAL'}
        </div>
      </div>

      <div className="traffic-stats">
        <div className="traffic-stat-item">
          <div className="traffic-stat-label">SIGNALS CLEARED</div>
          <div className="traffic-stat-value">{preemptedCount}</div>
        </div>
        <div className="traffic-stat-item">
          <div className="traffic-stat-label">TOTAL SIGNALS</div>
          <div className="traffic-stat-value">{totalSignals}</div>
        </div>
      </div>

      <div className="traffic-visual">
        <div className="traffic-flow-indicator">
          <div className="flow-label">TRAFFIC FLOW</div>
          <div className="flow-bar">
            <div
              className="flow-fill"
              style={{
                width: `${isEmergencyActive ? 100 : 60}%`,
                background: isEmergencyActive ? '#00ff88' : '#ffaa00',
              }}
            />
          </div>
          <div className="flow-percentage">
            {isEmergencyActive ? '100%' : '60%'}
          </div>
        </div>
      </div>

      {isEmergencyActive && (
        <div className="green-corridor-indicator">
          <div className="corridor-icon">🟢</div>
          <div className="corridor-text">GREEN CORRIDOR ACTIVE</div>
        </div>
      )}
    </div>
  )
}


