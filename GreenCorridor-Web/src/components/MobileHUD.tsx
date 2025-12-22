import { useGameStore } from '../stores/gameStore'
import './MobileHUD.css'

export default function MobileHUD() {
  const {
    timeRemaining,
    ambulanceSpeed,
    patient,
    emergencyType,
    isEmergencyActive,
    timeToHospital,
    ambulanceHeading,
  } = useGameStore()

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60)
    const secs = Math.floor(seconds % 60)
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }

  const overallHealth = Math.round(
    patient.consciousness * 0.3 +
    (patient.oxygenSaturation / 100) * 30 +
    (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
    (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
  )

  const isTimeCritical = timeRemaining < 120
  const isHealthCritical = overallHealth < 30

  const rpm = Math.min(6000, Math.max(0, ambulanceSpeed * 50))
  const rpmPercent = (rpm / 6000) * 100

  const fuel = Math.max(0, 100 - (useGameStore.getState().gameTime / 10) % 100)
  const fuelPercent = fuel

  const getGear = () => {
    if (ambulanceSpeed < 5) return 'N'
    if (ambulanceSpeed < 30) return '1'
    if (ambulanceSpeed < 50) return '2'
    if (ambulanceSpeed < 70) return '3'
    if (ambulanceSpeed < 90) return '4'
    return '5'
  }
  
  const currentGear = getGear()

  const getDirection = (heading: number) => {
    const normalized = ((heading * 180 / Math.PI) + 360) % 360
    if (normalized >= 337.5 || normalized < 22.5) return 'N'
    if (normalized >= 22.5 && normalized < 67.5) return 'NE'
    if (normalized >= 67.5 && normalized < 112.5) return 'E'
    if (normalized >= 112.5 && normalized < 157.5) return 'SE'
    if (normalized >= 157.5 && normalized < 202.5) return 'S'
    if (normalized >= 202.5 && normalized < 247.5) return 'SW'
    if (normalized >= 247.5 && normalized < 292.5) return 'W'
    return 'NW'
  }

  const gpsLat = (13.0827 + (useGameStore.getState().ambulancePosition[0] / 111000)).toFixed(5)
  const gpsLon = (80.2707 + (useGameStore.getState().ambulancePosition[2] / 111000)).toFixed(5)

  const distance = timeToHospital > 0 ? Math.round(timeToHospital * (ambulanceSpeed / 3.6)) : 0

  return (
    <div className="mobile-hud-container">
      {/* Top Bar */}
      <div className="mobile-top-bar">
        <div className="top-speed-display">
          <span className="top-speed-value">{Math.round(ambulanceSpeed)}</span>
          <span className="top-speed-unit">KPH</span>
        </div>
        <div className={`top-gear-display ${currentGear === 'N' ? 'neutral' : 'drive'}`}>
          {currentGear}
        </div>
        <div className={`top-timer ${isTimeCritical ? 'critical' : ''}`}>
          {formatTime(timeRemaining)}
        </div>
      </div>

      {/* Left Panel - Speed & Navigation */}
      <div className="mobile-left-panel">
        <div className="digital-speed-container">
          <div className="digital-speed-value">{Math.round(ambulanceSpeed)}</div>
          <div className="digital-speed-unit">KPH</div>
        </div>
        
        <div className="navigation-info">
          <div className="nav-item">
            <div className="nav-label">GPS</div>
            <div className="nav-value">{gpsLat}, {gpsLon}</div>
          </div>
          <div className="nav-item">
            <div className="nav-label">COMPASS</div>
            <div className="nav-value">{getDirection(ambulanceHeading)}</div>
          </div>
          <div className="nav-item">
            <div className="nav-label">ALT</div>
            <div className="nav-value">{Math.round(useGameStore.getState().ambulancePosition[1])}m</div>
          </div>
        </div>
      </div>

      {/* Right Panel - Vehicle Status */}
      <div className="mobile-right-panel">
        {/* RPM Gauge */}
        <div className="rpm-gauge-container">
          <div className="gauge-label">RPM</div>
          <div className="rpm-gauge">
            <svg className="rpm-gauge-svg" viewBox="0 0 120 120">
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
                className="gauge-fill"
                cx="60"
                cy="60"
                r="50"
                fill="none"
                stroke={rpmPercent > 80 ? '#FF3333' : rpmPercent > 60 ? '#FFAA00' : '#0066CC'}
                strokeWidth="6"
                strokeDasharray={`${(rpmPercent / 100) * 314} 314`}
                strokeDashoffset="78.5"
                transform="rotate(-90 60 60)"
                strokeLinecap="round"
              />
            </svg>
            <div className="rpm-value">{Math.round(rpm)}</div>
          </div>
        </div>

        {/* Fuel Gauge */}
          <div className="linear-gauge">
            <div className="gauge-label">FUEL</div>
            <div className="gauge-bar">
              <div
                className={`gauge-fill-bar fuel-gauge ${fuelPercent < 20 ? 'low' : fuelPercent < 40 ? 'medium' : 'high'}`}
                style={{ width: `${fuelPercent}%` }}
              />
            </div>
            <div className="gauge-value-text">{Math.round(fuel)}%</div>
          </div>

          {/* Temperature Gauge */}
          <div className="linear-gauge">
            <div className="gauge-label">TEMP</div>
            <div className="gauge-bar">
              <div
                className={`gauge-fill-bar temp-gauge ${ambulanceSpeed > 80 ? 'hot' : ambulanceSpeed > 50 ? 'warm' : 'cool'}`}
                style={{ width: `${Math.min(100, (ambulanceSpeed / 2) + 30)}%` }}
              />
            </div>
            <div className="gauge-value-text">{Math.round((ambulanceSpeed / 2) + 30)}°C</div>
          </div>
      </div>

      {/* Bottom Panel - Medical & Mission */}
      <div className="mobile-bottom-panel">
        <div className="medical-section">
          <div className="health-display">
            <div className="health-label">PATIENT HEALTH</div>
            <div className="health-bar-container">
            <div
              className={`health-bar-fill ${overallHealth > 70 ? 'high' : overallHealth > 30 ? 'medium' : 'low'}`}
              style={{ width: `${overallHealth}%` }}
            />
              <div className={`health-percentage ${isHealthCritical ? 'critical' : ''}`}>
                {overallHealth}%
              </div>
            </div>
          </div>

          <div className="vitals-display">
            <div className="vital-display-item">
              <div className="vital-icon">❤️</div>
              <div className="vital-value-display">
                <div className="vital-label-small">HR</div>
                <div className={`vital-number ${patient.heartRate > 100 || patient.heartRate < 60 ? 'warning' : ''}`}>
                  {Math.round(patient.heartRate)}
                </div>
              </div>
            </div>
            <div className="vital-display-item">
              <div className="vital-icon">🫁</div>
              <div className="vital-value-display">
                <div className="vital-label-small">O2</div>
                <div className={`vital-number ${patient.oxygenSaturation < 90 ? 'warning' : ''}`}>
                  {Math.round(patient.oxygenSaturation)}%
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="mission-section">
          <div className="mission-timer">
            <div className="mission-label">MISSION TIME</div>
            <div className={`mission-time-value ${isTimeCritical ? 'critical' : ''}`}>
              {formatTime(timeRemaining)}
            </div>
          </div>
          <div className="mission-objective">
            <div className="mission-label">OBJECTIVE</div>
            <div className="mission-text">{emergencyType}</div>
          </div>
          {distance > 0 && (
            <div className="hospital-distance">
              <div className="mission-label">HOSPITAL</div>
              <div className="distance-value">
                {distance > 1000 ? `${(distance / 1000).toFixed(1)}km` : `${distance}m`}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Center Steering Wheel Overlay */}
      <div className="steering-wheel-overlay">
        <div className="steering-wheel-icon">🚗</div>
      </div>

      {/* Emergency Mode Overlay */}
      {isEmergencyActive && (
        <div className="emergency-overlay">
          <div className="emergency-flash"></div>
          <div className="emergency-text">EMERGENCY MODE</div>
        </div>
      )}
    </div>
  )
}

