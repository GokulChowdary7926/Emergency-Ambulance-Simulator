import { useEffect } from 'react'
import { useGameStore } from '../stores/gameStore'
import { realtimeAPI } from '../services/RealtimeAPI'
import CommunicationPanel from './CommunicationPanel'
import VitalsMonitor from './VitalsMonitor'
import TrafficControlPanel from './TrafficControlPanel'
import LeafletMap from './LeafletMap'
import './HUD.css'

export default function HUD() {
  const {
    timeRemaining,
    ambulanceSpeed,
    ambulancePosition,
    patient,
    emergencyType,
    isEmergencyActive,
    timeToHospital,
  } = useGameStore()

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60)
    const secs = Math.floor(seconds % 60)
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }

  const getHealthColor = (health: number) => {
    if (health > 70) return '#00ff88'
    if (health > 30) return '#ffaa00'
    return '#ff3333'
  }

  const overallHealth = Math.round(
    patient.consciousness * 0.3 +
    (patient.oxygenSaturation / 100) * 30 +
    (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
    (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
  )

  const isTimeCritical = timeRemaining < 120
  const isHealthCritical = overallHealth < 30

  const rpm = Math.min(8000, Math.max(1000, ambulanceSpeed * 60))
  const rpmPercent = (rpm / 8000) * 100

  const fuel = Math.max(0, 100 - (useGameStore.getState().gameTime / 10) % 100)
  const fuelPercent = fuel

  const getGear = () => {
    if (ambulanceSpeed < 10) return 'N'
    if (ambulanceSpeed < 30) return '1'
    if (ambulanceSpeed < 50) return '2'
    if (ambulanceSpeed < 70) return '3'
    if (ambulanceSpeed < 90) return '4'
    return '5'
  }

  const distance = timeToHospital > 0 ? Math.round(timeToHospital * (ambulanceSpeed / 3.6)) : 0

  useEffect(() => {
    const updateRealtimeData = async () => {
      const lat = 13.0827 + (ambulancePosition[0] / 111000)
      const lon = 80.2707 + (ambulancePosition[2] / 111000)
      
      const weather = await realtimeAPI.getWeatherData(lat, lon)
      const traffic = await realtimeAPI.getTrafficData(lat, lon)
      
      useGameStore.setState({
        realtimeWeather: {
          temperature: weather.temperature,
          condition: weather.condition,
          visibility: weather.visibility,
        },
        realtimeTraffic: {
          density: traffic.density,
          flow: traffic.flow,
        },
      })
    }
    
    updateRealtimeData()
    const interval = setInterval(updateRealtimeData, 60000) // Update every minute
    return () => clearInterval(interval)
  }, [ambulancePosition])

  return (
    <div className="hud-container">
      {/* Top Title Bar */}
      <div className="hud-title-bar">
        <div className="title-text">EMERGENCY AMBULANCE SIMULATOR</div>
        <div className="title-subtitle">Green Corridor System</div>
      </div>

      {/* Left Dashboard - Gauges */}
      <div className="hud-dashboard-left">
        {/* Speed Gauge */}
        <div className="gauge-container">
          <div className="gauge-label">SPEED</div>
          <div className="gauge-value">{Math.round(ambulanceSpeed)}</div>
          <div className="gauge-unit">km/h</div>
          <div className="gauge-ring">
            <svg className="gauge-svg" viewBox="0 0 120 120">
              <circle
                className="gauge-background"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke="rgba(255,255,255,0.1)"
                strokeWidth="6"
              />
              <circle
                className="gauge-fill speed-gauge"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke={ambulanceSpeed > 80 ? '#ff3333' : ambulanceSpeed > 40 ? '#ffaa00' : '#00ff88'}
                strokeWidth="6"
                strokeDasharray={`${(ambulanceSpeed / 120) * 314} 314`}
                strokeDashoffset="78.5"
                transform="rotate(-90 60 60)"
                strokeLinecap="round"
              />
            </svg>
          </div>
        </div>

        {/* RPM Gauge */}
        <div className="gauge-container">
          <div className="gauge-label">RPM</div>
          <div className="gauge-value-small">{Math.round(rpm)}</div>
          <div className="gauge-ring">
            <svg className="gauge-svg" viewBox="0 0 120 120">
              <circle
                className="gauge-background"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke="rgba(255,255,255,0.1)"
                strokeWidth="6"
              />
              <circle
                className="gauge-fill rpm-gauge"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke={rpmPercent > 80 ? '#ff3333' : rpmPercent > 60 ? '#ffaa00' : '#00ff88'}
                strokeWidth="6"
                strokeDasharray={`${(rpmPercent / 100) * 314} 314`}
                strokeDashoffset="78.5"
                transform="rotate(-90 60 60)"
                strokeLinecap="round"
              />
            </svg>
          </div>
        </div>

        {/* Fuel Gauge */}
        <div className="gauge-container">
          <div className="gauge-label">FUEL</div>
          <div className="gauge-value-small">{Math.round(fuel)}%</div>
          <div className="gauge-ring">
            <svg className="gauge-svg" viewBox="0 0 120 120">
              <circle
                className="gauge-background"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke="rgba(255,255,255,0.1)"
                strokeWidth="6"
              />
              <circle
                className="gauge-fill fuel-gauge"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke={fuelPercent < 20 ? '#ff3333' : fuelPercent < 40 ? '#ffaa00' : '#00aaff'}
                strokeWidth="6"
                strokeDasharray={`${(fuelPercent / 100) * 314} 314`}
                strokeDashoffset="78.5"
                transform="rotate(-90 60 60)"
                strokeLinecap="round"
              />
            </svg>
          </div>
        </div>
      </div>

      {/* Center Info Panel */}
      <div className="hud-center-panel">
        <div className="info-row">
          <div className="info-item">
            <div className="info-label">TIME</div>
            <div className={`info-value ${isTimeCritical ? 'critical' : ''}`}>
              {formatTime(timeRemaining)}
            </div>
          </div>
          <div className="info-item">
            <div className="info-label">GEAR</div>
            <div className="info-value gear-display">{getGear()}</div>
          </div>
          <div className="info-item">
            <div className="info-label">DISTANCE</div>
            <div className="info-value">{distance}m</div>
          </div>
        </div>

        <div className="status-indicators">
          <div className={`status-indicator ${isEmergencyActive ? 'active' : ''}`}>
            <div className="status-icon">🚨</div>
            <div className="status-label">SIREN</div>
          </div>
          <div className={`status-indicator ${isEmergencyActive ? 'active' : ''}`}>
            <div className="status-icon">💡</div>
            <div className="status-label">LIGHTS</div>
          </div>
          <div className="status-indicator active">
            <div className="status-icon">📍</div>
            <div className="status-label">GPS</div>
          </div>
          <div className="status-indicator active">
            <div className="status-icon">📻</div>
            <div className="status-label">RADIO</div>
          </div>
        </div>
      </div>

      {/* Right Panel - Patient & Map */}
      <div className="hud-right-panel">
        {/* Patient Health */}
        <div className={`patient-card ${isHealthCritical ? 'critical' : ''}`}>
          <div className="patient-header">
            <div className="patient-icon">🏥</div>
            <div>
              <div className="patient-condition">{emergencyType}</div>
              <div className="patient-name">PATIENT STATUS</div>
            </div>
          </div>

          <div className="health-bar-container">
            <div className="health-bar-label">
              <span>HEALTH</span>
              <span className={`health-percentage ${isHealthCritical ? 'critical-text' : ''}`}>
                {overallHealth}%
              </span>
            </div>
            <div className="health-bar-wrapper">
              <div
                className="health-bar-fill"
                style={{
                  width: `${overallHealth}%`,
                  background: `linear-gradient(90deg, ${getHealthColor(overallHealth)} 0%, ${getHealthColor(overallHealth)}dd 100%)`,
                  boxShadow: `0 0 20px ${getHealthColor(overallHealth)}80`,
                }}
              />
              {isHealthCritical && <div className="health-bar-pulse"></div>}
            </div>
          </div>

          <div className="vitals-mini">
            <div className="vital-mini">
              <span className="vital-label-mini">HR</span>
              <span className={`vital-value-mini ${patient.heartRate > 100 || patient.heartRate < 60 ? 'warning' : ''}`}>
                {Math.round(patient.heartRate)}
              </span>
            </div>
            <div className="vital-mini">
              <span className="vital-label-mini">O2</span>
              <span className={`vital-value-mini ${patient.oxygenSaturation < 90 ? 'warning' : ''}`}>
                {Math.round(patient.oxygenSaturation)}%
              </span>
            </div>
            <div className="vital-mini">
              <span className="vital-label-mini">BP</span>
              <span className="vital-value-mini">
                {Math.round(patient.bloodPressureSystolic)}/{Math.round(patient.bloodPressureDiastolic)}
              </span>
            </div>
          </div>
        </div>

            {/* Leaflet Map Minimap */}
            <div className="minimap-container">
              <div className="minimap-label">MAP</div>
              <div className="minimap-wrapper">
                <LeafletMap />
              </div>
            </div>
      </div>

      {/* Bottom Control Hints */}
      <div className="hud-controls-bottom">
        <div className="control-hint">
          <kbd>W</kbd><kbd>A</kbd><kbd>S</kbd><kbd>D</kbd>
          <span>Drive</span>
        </div>
        <div className="control-hint">
          <kbd>SPACE</kbd>
          <span>Emergency</span>
        </div>
        <div className="control-hint">
          <kbd>H</kbd>
          <span>Help</span>
        </div>
        <div className="control-hint">
          <kbd>ESC</kbd>
          <span>Pause</span>
        </div>
      </div>

      {/* Speed Limit Indicator */}
      <div className="speed-limit-indicator">
        <div className="speed-limit-label">LIMIT</div>
        <div className="speed-limit-value">{isEmergencyActive ? '∞' : '80'}</div>
        <div className="speed-limit-unit">km/h</div>
      </div>

      {/* Communication Panel */}
      <CommunicationPanel />

      {/* Vitals Monitor */}
      <VitalsMonitor />

      {/* Traffic Control Panel */}
      <TrafficControlPanel />

      {/* Treatment Status */}
      {patient && (
        <div className="treatment-panel">
          <div className="treatment-header">TREATMENT STATUS</div>
          <div className="treatment-items">
            <div className="treatment-item">
              <div className={`treatment-indicator ${patient.oxygenApplied ? 'active' : ''}`}>
                {patient.oxygenApplied ? '✓' : '✗'}
              </div>
              <div className="treatment-label">OXYGEN</div>
            </div>
            <div className="treatment-item">
              <div className={`treatment-indicator ${patient.bleedingControlled ? 'active' : ''}`}>
                {patient.bleedingControlled ? '✓' : '✗'}
              </div>
              <div className="treatment-label">BLEEDING CONTROL</div>
            </div>
            <div className="treatment-item">
              <div className={`treatment-indicator ${patient.cprInProgress ? 'active' : ''}`}>
                {patient.cprInProgress ? '✓' : '✗'}
              </div>
              <div className="treatment-label">CPR</div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
