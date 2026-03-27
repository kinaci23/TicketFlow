export interface Ticket {
  ticketId: number;
  title: string;
  description: string;
  status: string;
  urgency: string;
  createdAt: Date;
  userName: string;
  predictedCategoryId?: number; 
  finalCategoryId?: number;
  adminResponse?: string | null; 
}

export interface TicketCreateDto {
  title: string;
  description: string;
  urgency: string; 
}

export interface TicketUpdateDto {
  ticketId: number;
  status: string;
  finalCategoryId: number;
  adminResponse: string | null; 
} 

export interface TicketMessage {
  messageId: number;
  ticketId: number;
  userId: number;
  messageText: string;
  createdAt: string | Date;
  senderName: string;
  senderRoleId: number; 
}

export interface AddMessageDto {
  ticketId: number;
  messageText: string;
}