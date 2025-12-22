export type GameCommand = 
  | 'TURN_LEFT' 
  | 'TURN_RIGHT' 
  | 'ACCELERATE' 
  | 'BRAKE' 
  | 'TOGGLE_SIREN' 
  | 'STOP' 
  | 'EMERGENCY_MODE'
  | 'UNKNOWN'

export interface VoiceControllerConfig {
  continuous: boolean
  interimResults: boolean
  lang: string
}

export class VoiceController {
  private recognition: any
  private isListening: boolean = false
  private commandCallback: ((command: GameCommand) => void) | null = null
  private config: VoiceControllerConfig

  constructor(config: Partial<VoiceControllerConfig> = {}) {
    this.config = {
      continuous: config.continuous ?? true,
      interimResults: config.interimResults ?? false,
      lang: config.lang ?? 'en-US'
    }
  }

  initialize(): boolean {
    if (typeof window === 'undefined') return false

    const SpeechRecognition = (window as any).SpeechRecognition || 
                              (window as any).webkitSpeechRecognition ||
                              (window as any).mozSpeechRecognition ||
                              (window as any).msSpeechRecognition

    if (!SpeechRecognition) {
      console.warn('Speech recognition not supported in this browser')
      return false
    }

    this.recognition = new SpeechRecognition()
    this.recognition.continuous = this.config.continuous
    this.recognition.interimResults = this.config.interimResults
    this.recognition.lang = this.config.lang

    this.recognition.onresult = (event: any) => {
      const transcript = event.results[event.results.length - 1][0].transcript.toLowerCase()
      const command = this.parseCommand(transcript)
      
      if (command !== 'UNKNOWN' && this.commandCallback) {
        this.commandCallback(command)
      }
    }

    this.recognition.onerror = (event: any) => {
      console.warn('Speech recognition error:', event.error)
    }

    this.recognition.onend = () => {
      if (this.isListening) {
        this.start()
      }
    }

    return true
  }

  start(): boolean {
    if (!this.recognition) {
      if (!this.initialize()) {
        return false
      }
    }

    try {
      this.recognition.start()
      this.isListening = true
      return true
    } catch (error) {
      console.warn('Failed to start voice recognition:', error)
      return false
    }
  }

  stop(): void {
    if (this.recognition && this.isListening) {
      this.isListening = false
      this.recognition.stop()
    }
  }

  onCommand(callback: (command: GameCommand) => void): void {
    this.commandCallback = callback
  }

  private parseCommand(transcript: string): GameCommand {
    const normalized = transcript.trim().toLowerCase()

    if (normalized.includes('turn left') || normalized.includes('go left')) {
      return 'TURN_LEFT'
    }
    if (normalized.includes('turn right') || normalized.includes('go right')) {
      return 'TURN_RIGHT'
    }
    if (normalized.includes('accelerate') || normalized.includes('speed up') || normalized.includes('go faster')) {
      return 'ACCELERATE'
    }
    if (normalized.includes('brake') || normalized.includes('slow down') || normalized.includes('stop')) {
      return 'BRAKE'
    }
    if (normalized.includes('siren') || normalized.includes('emergency lights')) {
      return 'TOGGLE_SIREN'
    }
    if (normalized.includes('emergency mode') || normalized.includes('activate emergency')) {
      return 'EMERGENCY_MODE'
    }
    if (normalized.includes('stop') && !normalized.includes('slow')) {
      return 'STOP'
    }

    return 'UNKNOWN'
  }

  isSupported(): boolean {
    return typeof window !== 'undefined' && (
      'SpeechRecognition' in window ||
      'webkitSpeechRecognition' in window ||
      'mozSpeechRecognition' in window ||
      'msSpeechRecognition' in window
    )
  }

  getIsListening(): boolean {
    return this.isListening
  }
}

