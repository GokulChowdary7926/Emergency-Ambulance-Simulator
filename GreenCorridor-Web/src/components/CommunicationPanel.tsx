import { useState, useEffect } from 'react'
import { useGameStore } from '../stores/gameStore'
import './CommunicationPanel.css'

export default function CommunicationPanel() {
  const { currentMission, patient, ambulanceSpeed, isEmergencyActive } = useGameStore()
  const [radioMessages, setRadioMessages] = useState<string[]>([])
  const [dispatchMessage, setDispatchMessage] = useState('')

  useEffect(() => {
    const messages = [
      'Dispatch to Ambulance 7, proceed to emergency.',
      'Hospital notified of your ETA.',
      'Traffic control standing by.',
      'Patient condition update required.',
      'Clear to proceed through intersection.',
      'Weather update: Clear conditions.',
      'Backup unit en route to your location.',
    ]

    const interval = setInterval(() => {
      if (Math.random() > 0.7) {
        const message = messages[Math.floor(Math.random() * messages.length)]
        setRadioMessages((prev) => [...prev.slice(-4), message])
      }
    }, 5000)

    return () => clearInterval(interval)
  }, [])

  useEffect(() => {
    if (!patient) return

    const overallHealth = Math.round(
      patient.consciousness * 0.3 +
      (patient.oxygenSaturation / 100) * 30 +
      (Math.max(0, 200 - patient.heartRate) / 100) * 20 +
      (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20
    )

    if (overallHealth < 30) {
      setDispatchMessage('URGENT: Patient critical, expedite!')
    } else if (overallHealth < 50) {
      setDispatchMessage('PRIORITY: Patient condition deteriorating')
    } else {
      setDispatchMessage('STATUS: Proceeding to hospital')
    }
  }, [patient])

  return (
    <div className="communication-panel">
      <div className="comm-header">
        <div className="comm-title">📻 COMMUNICATION</div>
        <div className={`comm-status ${isEmergencyActive ? 'active' : ''}`}>
          {isEmergencyActive ? 'EMERGENCY ACTIVE' : 'STAND BY'}
        </div>
      </div>

      <div className="dispatch-section">
        <div className="dispatch-label">DISPATCH</div>
        <div className={`dispatch-message ${patient && Math.round(patient.consciousness * 0.3 + (patient.oxygenSaturation / 100) * 30 + (Math.max(0, 200 - patient.heartRate) / 100) * 20 + (Math.max(0, 140 - patient.bloodPressureSystolic) / 100) * 20) < 30 ? 'critical' : ''}`}>
          {dispatchMessage}
        </div>
      </div>

      <div className="radio-section">
        <div className="radio-label">RADIO</div>
        <div className="radio-messages">
          {radioMessages.map((msg, i) => (
            <div key={i} className="radio-message">
              <span className="radio-time">{new Date().toLocaleTimeString()}</span>
              <span className="radio-text">{msg}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="comm-footer">
        <div className="comm-indicator">
          <div className={`comm-dot ${isEmergencyActive ? 'active' : ''}`}></div>
          <span>RADIO ACTIVE</span>
        </div>
      </div>
    </div>
  )
}


