import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Ticket, TicketCreateDto, TicketUpdateDto, TicketMessage, AddMessageDto } from '../models/ticket.model';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = 'https://localhost:7139/api/Tickets';

  constructor(private http: HttpClient) { }

  // Get My Tickets
  getMyTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/my-tickets`);
  }

  // Get All Tickets (Admin)
  getAllTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/all-tickets`);
  }

  // Create Ticket
  createTicket(ticketData: TicketCreateDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, ticketData);
  }

  // Get Ticket By Id
  getTicketById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiUrl}/${id}`);
  }

  // Update Ticket
  updateTicket(ticketData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update`, ticketData);
  }

  // Get Ticket Messages
  getTicketMessages(ticketId: number): Observable<TicketMessage[]> {
    return this.http.get<TicketMessage[]>(`${this.apiUrl}/${ticketId}/messages`);
  }

  // Add Ticket Message
  addTicketMessage(ticketId: number, messageDto: AddMessageDto): Observable<TicketMessage> {
    return this.http.post<TicketMessage>(`${this.apiUrl}/${ticketId}/messages`, messageDto);
  }
}
