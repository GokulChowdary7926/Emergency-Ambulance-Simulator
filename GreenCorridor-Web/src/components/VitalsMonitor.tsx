import { useGameStore } from '../stores/gameStore'
import './VitalsMonitor.css'

export default function VitalsMonitor() {
  const { patient } = useGameStore()

  if (!patient) return null

  const getVitalColor = (value: number, normalMin: number, normalMax: number) => {
    if (value >= normalMin && value <= normalMax) return '#00ff88'
    if (value < normalMin * 0.7 || value > normalMax * 1.3) return '#ff3333'
    return '#ffaa00'
  }

  const hrColor = getVitalColor(patient.heartRate, 60, 100)
  const o2Color = getVitalColor(patient.oxygenSaturation, 95, 100)
  const bpColor = getVitalColor(patient.bloodPressureSystolic, 90, 140)

  return (
    <div className="vitals-monitor">
      <div className="vitals-header">
        <div className="vitals-title">🫀 VITALS MONITOR</div>
        <div className="vitals-status">LIVE</div>
      </div>

      <div className="vitals-grid">
        <div className="vital-item">
          <div className="vital-icon">❤️</div>
          <div className="vital-label">HEART RATE</div>
          <div className="vital-value" style={{ color: hrColor }}>
            {Math.round(patient.heartRate)} BPM
          </div>
          <div className="vital-bar">
            <div
              className="vital-bar-fill"
              style={{
                width: `${Math.min(100, (patient.heartRate / 200) * 100)}%`,
                background: hrColor,
              }}
            />
          </div>
        </div>

        <div className="vital-item">
          <div className="vital-icon">🫁</div>
          <div className="vital-label">OXYGEN SAT</div>
          <div className="vital-value" style={{ color: o2Color }}>
            {Math.round(patient.oxygenSaturation)}%
          </div>
          <div className="vital-bar">
            <div
              className="vital-bar-fill"
              style={{
                width: `${patient.oxygenSaturation}%`,
                background: o2Color,
              }}
            />
          </div>
        </div>

        <div className="vital-item">
          <div className="vital-icon">🩸</div>
          <div className="vital-label">BLOOD PRESSURE</div>
          <div className="vital-value" style={{ color: bpColor }}>
            {Math.round(patient.bloodPressureSystolic)}/{Math.round(patient.bloodPressureDiastolic)}
          </div>
          <div className="vital-bar">
            <div
              className="vital-bar-fill"
              style={{
                width: `${Math.min(100, (patient.bloodPressureSystolic / 200) * 100)}%`,
                background: bpColor,
              }}
            />
          </div>
        </div>

        <div className="vital-item">
          <div className="vital-icon">🧠</div>
          <div className="vital-label">CONSCIOUSNESS</div>
          <div className="vital-value" style={{ color: patient.consciousness > 50 ? '#00ff88' : '#ff3333' }}>
            {Math.round(patient.consciousness)}%
          </div>
          <div className="vital-bar">
            <div
              className="vital-bar-fill"
              style={{
                width: `${patient.consciousness}%`,
                background: patient.consciousness > 50 ? '#00ff88' : '#ff3333',
              }}
            />
          </div>
        </div>
      </div>

      <div className="vitals-alerts">
        {patient.requiresCPR && (
          <div className="vital-alert critical">
            ⚠️ CPR REQUIRED
          </div>
        )}
        {patient.requiresOxygen && !patient.oxygenApplied && (
          <div className="vital-alert warning">
            ⚠️ OXYGEN NEEDED
          </div>
        )}
        {patient.isBleeding && !patient.bleedingControlled && (
          <div className="vital-alert critical">
            ⚠️ BLEEDING ACTIVE
          </div>
        )}
      </div>
    </div>
  )
}


