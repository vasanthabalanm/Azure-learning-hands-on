import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Appointment {
  id: number;
  patientName: string;
  status: string;
  assignedDoctorName?: string;
  pickedByDoctorName?: string;
  bookedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = 'http://localhost:5000/api/appointment';

  constructor(private http: HttpClient) {}

  bookAppointment(patientName: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.apiUrl}/book`, { patientName });
  }

  mapToDoctor(appointmentId: number, doctorName: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.apiUrl}/map`, { appointmentId, doctorName });
  }

  pickPatient(appointmentId: number, doctorName: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.apiUrl}/pick`, { appointmentId, doctorName });
  }

  getAllAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.apiUrl}/all`);
  }

  getDoctorAppointments(doctorName: string): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.apiUrl}/doctor/${doctorName}`);
  }
}
