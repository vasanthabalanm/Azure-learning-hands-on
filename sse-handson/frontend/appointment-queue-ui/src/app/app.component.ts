import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { SseService } from './services/sse.service';
import { AppointmentService, Appointment } from './services/appointment.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  template: `
    <div class="app">
      <!-- Header with SSE Status -->
      <div class="header">
        <h1>🏥 Real-Time SSE Appointment Queue</h1>
        <div class="header-info">
          <span [class.connected]="isConnected" [class.disconnected]="!isConnected" class="sse-badge">
            {{ isConnected ? '🟢 SSE Connected' : '🔴 SSE Disconnected' }}
          </span>
          <span class="subtitle">Open 3 tabs (Patient, Staff, Doctor) to see real-time SSE updates!</span>
        </div>
      </div>

      <!-- Role Tabs -->
      <div class="tabs-container">
        <button
          *ngFor="let tab of roleTabs"
          [class.active]="activeRole === tab.id"
          (click)="switchRole(tab.id)"
          class="tab-button"
        >
          {{ tab.icon }} {{ tab.label }}
        </button>
      </div>

      <!-- Main Content Area -->
      <div class="content-container">

        <!-- PATIENT TAB -->
        <div *ngIf="activeRole === 'patient'" class="role-screen patient-screen">
          <div class="screen-header">
            <h2>👤 Patient Screen</h2>
            <p class="description">Book appointments and track their status in real-time</p>
          </div>

          <div class="action-section">
            <h3>📅 Book New Appointment</h3>
            <div class="action-box">
              <input
                [(ngModel)]="patientName"
                placeholder="Enter your name"
                class="input-field"
              />
              <button (click)="bookAppointment()" class="btn btn-primary">
                📝 Book Appointment
              </button>
            </div>
          </div>

          <div class="appointments-section">
            <h3>📋 Your Appointments</h3>
            <div *ngIf="appointments.length === 0" class="empty-state">
              <p>No appointments yet. Book one above!</p>
            </div>
            <div *ngFor="let apt of appointments" class="appointment-card">
              <div class="card-header">
                <span class="apt-id">#{{ apt.id }}</span>
                <span [class]="'status status-' + apt.status">{{ getStatusText(apt.status) }}</span>
              </div>
              <div class="card-body">
                <p><strong>Name:</strong> {{ apt.patientName }}</p>
                <p *ngIf="apt.assignedDoctorName"><strong>Doctor:</strong> Dr. {{ apt.assignedDoctorName }}</p>
                <p><strong>Booked:</strong> {{ apt.bookedAt | date: 'short' }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- STAFF TAB -->
        <div *ngIf="activeRole === 'staff'" class="role-screen staff-screen">
          <div class="screen-header">
            <h2>👔 Staff Screen</h2>
            <p class="description">Manage appointments and assign patients to doctors</p>
          </div>

          <div class="action-section">
            <h3>➡️ Assign Patient to Doctor</h3>
            <div class="action-box">
              <select [(ngModel)]="selectedAppointmentId" class="select-field">
                <option value="">Select Appointment</option>
                <option *ngFor="let apt of pendingAppointments" [value]="apt.id">
                  #{{ apt.id }} - {{ apt.patientName }} (Pending)
                </option>
              </select>
              <input
                [(ngModel)]="selectedDoctor"
                placeholder="Doctor name (e.g., Smith)"
                class="input-field"
              />
              <button (click)="mapToDoctor()" class="btn btn-success">
                ✅ Assign to Doctor
              </button>
            </div>
          </div>

          <div class="queue-section">
            <h3>📊 Appointment Queue</h3>
            <div *ngIf="appointments.length === 0" class="empty-state">
              <p>No appointments in queue</p>
            </div>
            <div *ngFor="let apt of appointments" class="appointment-card" [class]="apt.status | lowercase">
              <div class="card-header">
                <span class="apt-id">#{{ apt.id }}</span>
                <span [class]="'status status-' + apt.status">{{ getStatusText(apt.status) }}</span>
              </div>
              <div class="card-body">
                <p><strong>Patient:</strong> {{ apt.patientName }}</p>
                <p *ngIf="apt.assignedDoctorName"><strong>Assigned to:</strong> Dr. {{ apt.assignedDoctorName }}</p>
                <p><strong>Booked:</strong> {{ apt.bookedAt | date: 'short' }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- DOCTOR TAB -->
        <div *ngIf="activeRole === 'doctor'" class="role-screen doctor-screen">
          <div class="screen-header">
            <h2>👨‍⚕️ Doctor Screen</h2>
            <p class="description">Pick patients from your queue in real-time</p>
          </div>

          <div class="queue-section">
            <div *ngIf="doctorQueues.length === 0" class="empty-state">
              <p>No patients assigned yet</p>
            </div>
            <div *ngFor="let doctor of doctorQueues" class="doctor-card">
              <div class="doctor-card-header">
                <h3>👨‍⚕️ Dr. {{ doctor.name }}</h3>
                <span class="doctor-badge">Available: {{ doctor.availableCount }} | Picked: {{ doctor.pickedCount }}</span>
              </div>

              <!-- Available Queue -->
              <div class="sub-section">
                <h4>📋 Waiting in Queue ({{ doctor.availableCount }})</h4>
                <div *ngIf="doctor.availableQueue.length === 0" class="empty-state">
                  <p>No patients waiting</p>
                </div>
                <div *ngFor="let apt of doctor.availableQueue" class="appointment-card pickable">
                  <div class="card-header">
                    <span class="apt-id">#{{ apt.id }}</span>
                    <button (click)="pickPatient(apt.id, doctor.name)" class="btn btn-pick">
                      ✋ Pick Patient
                    </button>
                  </div>
                  <div class="card-body">
                    <p><strong>Name:</strong> {{ apt.patientName }}</p>
                    <p><strong>Booked:</strong> {{ apt.bookedAt | date: 'short' }}</p>
                  </div>
                </div>
              </div>

              <!-- Picked Patients -->
              <div class="sub-section">
                <h4>💊 Under Examination ({{ doctor.pickedCount }})</h4>
                <div *ngIf="doctor.pickedPatients.length === 0" class="empty-state">
                  <p>No patients being examined</p>
                </div>
                <div *ngFor="let apt of doctor.pickedPatients" class="appointment-card picked">
                  <div class="card-header">
                    <span class="apt-id">#{{ apt.id }}</span>
                    <span class="status status-PickedByDoctor">Examining</span>
                  </div>
                  <div class="card-body">
                    <p><strong>Name:</strong> {{ apt.patientName }}</p>
                    <p><strong>Booked:</strong> {{ apt.bookedAt | date: 'short' }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- SSE Event Log -->
      <div class="event-log-container">
        <h3>📡 Real-Time SSE Event Log</h3>
        <p class="log-subtitle">Latest events received from server (auto-updates via SSE)</p>
        <div class="event-log">
          <div *ngIf="recentEvents.length === 0" class="no-events">
            <p>Waiting for events...</p>
          </div>
          <div *ngFor="let event of recentEvents" class="event-item">
            <span [class]="'event-badge event-' + event.eventType">{{ event.eventType }}</span>
            <div>
              <div class="event-time">{{ event.timestamp | date: 'HH:mm:ss' }}</div>
              <div class="event-data">{{ event.data | json }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    * {
      box-sizing: border-box;
    }

    .app {
      background: linear-gradient(135deg, #667eea, #764ba2);
      min-height: 100vh;
      padding: 20px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .header {
      text-align: center;
      margin-bottom: 20px;
      padding: 20px;
      background: white;
      border-radius: 12px;
      box-shadow: 0 8px 16px rgba(0,0,0,0.15);
    }

    .header h1 {
      margin: 0 0 10px;
      color: #333;
      font-size: 32px;
    }

    .header-info {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 20px;
      flex-wrap: wrap;
    }

    .sse-badge {
      padding: 8px 16px;
      border-radius: 20px;
      font-weight: bold;
      font-size: 14px;
    }

    .sse-badge.connected {
      background: #d4edda;
      color: #155724;
    }

    .sse-badge.disconnected {
      background: #f8d7da;
      color: #721c24;
    }

    .subtitle {
      font-size: 14px;
      color: #666;
      font-style: italic;
    }

    .tabs-container {
      display: flex;
      gap: 10px;
      margin-bottom: 20px;
      background: white;
      padding: 10px;
      border-radius: 12px;
      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }

    .tab-button {
      flex: 1;
      padding: 12px 20px;
      border: none;
      background: #f0f0f0;
      color: #333;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      border-radius: 8px;
      transition: all 0.3s;
    }

    .tab-button:hover {
      background: #e0e0e0;
    }

    .tab-button.active {
      background: #007bff;
      color: white;
      box-shadow: 0 4px 8px rgba(0,123,255,0.3);
    }

    .content-container {
      background: white;
      border-radius: 12px;
      padding: 30px;
      margin-bottom: 20px;
      box-shadow: 0 8px 16px rgba(0,0,0,0.15);
      min-height: 500px;
    }

    .role-screen {
      animation: fadeIn 0.3s ease-in;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .screen-header {
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 3px solid #f0f0f0;
    }

    .screen-header h2 {
      margin: 0 0 8px;
      font-size: 28px;
      color: #333;
    }

    .screen-header .description {
      margin: 0;
      color: #666;
      font-size: 14px;
    }

    .action-section {
      margin-bottom: 30px;
      padding: 20px;
      background: #f8f9fa;
      border-radius: 8px;
      border-left: 4px solid #007bff;
    }

    .action-section h3 {
      margin: 0 0 15px;
      font-size: 18px;
      color: #333;
    }

    .action-box {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .input-field,
    .select-field {
      padding: 12px 16px;
      border: 2px solid #ddd;
      border-radius: 6px;
      font-size: 14px;
      transition: border-color 0.3s;
    }

    .input-field:focus,
    .select-field:focus {
      outline: none;
      border-color: #007bff;
    }

    .btn {
      padding: 12px 20px;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background: #0056b3;
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0,123,255,0.3);
    }

    .btn-success {
      background: #28a745;
      color: white;
    }

    .btn-success:hover {
      background: #218838;
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(40,167,69,0.3);
    }

    .btn-pick {
      background: #17a2b8;
      color: white;
      padding: 8px 12px;
      font-size: 12px;
    }

    .btn-pick:hover {
      background: #138496;
    }

    .appointments-section,
    .queue-section {
      margin-bottom: 30px;
    }

    .appointments-section h3,
    .queue-section h4 {
      font-size: 18px;
      color: #333;
      margin: 0 0 15px;
    }

    .queue-section h4 {
      font-size: 16px;
      color: #555;
    }

    .doctor-card {
      background: #f8f9fa;
      padding: 20px;
      margin-bottom: 20px;
      border-radius: 8px;
      border-left: 4px solid #dc3545;
    }

    .doctor-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding-bottom: 15px;
      border-bottom: 2px solid #ddd;
    }

    .doctor-card-header h3 {
      margin: 0;
      font-size: 20px;
      color: #333;
    }

    .doctor-badge {
      background: #dc3545;
      color: white;
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
    }

    .appointment-card {
      background: white;
      padding: 15px;
      margin: 10px 0;
      border-radius: 8px;
      border-left: 4px solid #007bff;
      transition: all 0.3s;
    }

    .appointment-card:hover {
      transform: translateX(4px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .appointment-card.pending { border-left-color: #ffc107; background: #fff9e6; }
    .appointment-card.assigned { border-left-color: #17a2b8; background: #e6f7ff; }
    .appointment-card.pickable { border-left-color: #28a745; background: #e8f5e9; }
    .appointment-card.picked { border-left-color: #dc3545; background: #ffe6e6; }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
    }

    .apt-id {
      font-weight: bold;
      color: #007bff;
      font-size: 14px;
    }

    .status {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
    }

    .status-Pending {
      background: #fff3cd;
      color: #856404;
    }

    .status-MappedToDoctor {
      background: #d1ecf1;
      color: #0c5460;
    }

    .status-PickedByDoctor {
      background: #d4edda;
      color: #155724;
    }

    .card-body {
      font-size: 13px;
      color: #555;
    }

    .card-body p {
      margin: 4px 0;
    }

    .empty-state {
      padding: 30px;
      text-align: center;
      color: #999;
      font-style: italic;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .sub-section {
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .sub-section h4 {
      font-size: 14px;
      color: #555;
      margin: 0 0 10px;
    }

    .event-log-container {
      background: #1e1e1e;
      color: white;
      border-radius: 12px;
      padding: 20px;
      box-shadow: 0 8px 16px rgba(0,0,0,0.3);
    }

    .event-log-container h3 {
      margin: 0 0 5px;
      color: #4fc3f7;
      font-size: 18px;
    }

    .log-subtitle {
      margin: 0 0 15px;
      color: #aaa;
      font-size: 12px;
    }

    .event-log {
      max-height: 250px;
      overflow-y: auto;
    }

    .no-events {
      padding: 20px;
      text-align: center;
      color: #666;
    }

    .event-item {
      background: #2d2d2d;
      padding: 12px;
      margin: 8px 0;
      border-radius: 6px;
      border-left: 3px solid #4fc3f7;
      display: flex;
      align-items: flex-start;
      gap: 12px;
    }

    .event-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 11px;
      font-weight: bold;
      white-space: nowrap;
      text-transform: uppercase;
      min-width: fit-content;
    }

    .event-appointment_booked { background: #4fc3f7; color: black; }
    .event-patient_mapped { background: #81c784; color: white; }
    .event-patient_picked { background: #e57373; color: white; }
    .event-connected { background: #9575cd; color: white; }

    .event-type {
      color: #4fc3f7;
      font-weight: bold;
      margin-right: 10px;
    }

    .event-time {
      color: #aaa;
      font-size: 12px;
    }

    .event-data {
      margin: 8px 0 0 0;
      font-size: 11px;
      color: #90ee90;
      background: #1a1a1a;
      padding: 8px;
      border-radius: 4px;
      overflow-x: auto;
    }

    @media (max-width: 768px) {
      .tabs-container { flex-direction: column; }
      .tab-button { padding: 10px; }
      .content-container { padding: 15px; }
      .card-header { flex-direction: column; align-items: flex-start; gap: 8px; }
      .btn-pick { align-self: flex-start; }
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  isConnected: boolean = false;
  patientName: string = '';
  selectedAppointmentId: string = '';
  selectedDoctor: string = '';

  appointments: Appointment[] = [];
  pendingAppointments: Appointment[] = [];
  assignedAppointments: Appointment[] = [];
  doctorQueues: DoctorQueue[] = [];
  recentEvents: any[] = [];

  activeRole: string = 'patient';
  roleTabs = [
    { id: 'patient', label: 'Patient', icon: '👤' },
    { id: 'staff', label: 'Staff', icon: '👔' },
    { id: 'doctor', label: 'Doctor', icon: '👨‍⚕️' }
  ];

  private sseSubscription?: Subscription;
  private connectionSubscription?: Subscription;

  constructor(
    private sseService: SseService,
    private appointmentService: AppointmentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.sseService.connect();

    this.connectionSubscription = this.sseService.connectionStatus$.subscribe(
      status => {
        this.isConnected = status;
        this.cdr.detectChanges();
      }
    );

    this.sseSubscription = this.sseService.events$.subscribe(event => {
      this.handleSseEvent(event);
    });

    this.loadAppointments();
  }

  ngOnDestroy() {
    this.sseSubscription?.unsubscribe();
    this.connectionSubscription?.unsubscribe();
    this.sseService.disconnect();
  }

  switchRole(roleId: string) {
    this.activeRole = roleId;
  }

  handleSseEvent(event: any) {
    console.log('🎯 SSE Event received:', event);

    this.recentEvents.unshift(event);
    if (this.recentEvents.length > 10) {
      this.recentEvents.pop();
    }

    this.loadAppointments();
    this.cdr.detectChanges();
  }

  bookAppointment() {
    if (!this.patientName) return;

    this.appointmentService.bookAppointment(this.patientName).subscribe({
      next: () => {
        console.log('✅ Appointment booked');
        this.patientName = '';
      },
      error: (err) => console.error('❌ Failed to book:', err)
    });
  }

  mapToDoctor() {
    if (!this.selectedAppointmentId || !this.selectedDoctor) return;

    this.appointmentService.mapToDoctor(
      parseInt(this.selectedAppointmentId),
      this.selectedDoctor
    ).subscribe({
      next: () => {
        console.log('✅ Mapped to doctor');
        this.selectedAppointmentId = '';
        this.selectedDoctor = '';
      },
      error: (err) => console.error('❌ Failed to map:', err)
    });
  }

  pickPatient(appointmentId: number, doctorName: string) {
    this.appointmentService.pickPatient(appointmentId, doctorName).subscribe({
      next: () => {
        console.log('✅ Patient picked');
      },
      error: (err) => {
        console.error('❌ Failed to pick:', err);
        alert('Failed to pick patient. Another doctor may have already picked this patient.');
      }
    });
  }

  loadAppointments() {
    this.appointmentService.getAllAppointments().subscribe({
      next: (apts) => {
        this.appointments = apts;
        this.pendingAppointments = apts.filter(a => a.status === 'Pending');
        this.assignedAppointments = apts.filter(a => a.status === 'MappedToDoctor');
        this.updateDoctorQueues(apts);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('❌ Failed to load appointments:', err)
    });
  }

  updateDoctorQueues(appointments: Appointment[]) {
    const doctorMap = new Map<string, DoctorQueue>();

    appointments.forEach(apt => {
      if (apt.assignedDoctorName) {
        if (!doctorMap.has(apt.assignedDoctorName)) {
          doctorMap.set(apt.assignedDoctorName, {
            name: apt.assignedDoctorName,
            availableQueue: [],
            pickedPatients: [],
            availableCount: 0,
            pickedCount: 0
          });
        }

        const queue = doctorMap.get(apt.assignedDoctorName)!;
        if (apt.status === 'MappedToDoctor') {
          queue.availableQueue.push(apt);
          queue.availableCount++;
        } else if (apt.status === 'PickedByDoctor') {
          queue.pickedPatients.push(apt);
          queue.pickedCount++;
        }
      }
    });

    this.doctorQueues = Array.from(doctorMap.values());
  }

  getStatusText(status: string): string {
    const statusMap: any = {
      'Pending': 'Waiting',
      'MappedToDoctor': 'Assigned',
      'PickedByDoctor': 'Examining'
    };
    return statusMap[status] || status;
  }
}

interface DoctorQueue {
  name: string;
  availableQueue: Appointment[];
  pickedPatients: Appointment[];
  availableCount: number;
  pickedCount: number;
}
