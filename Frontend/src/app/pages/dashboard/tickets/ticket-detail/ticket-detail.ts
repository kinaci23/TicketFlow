import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TicketService } from '../../../../core/services/ticket.service';
import { Ticket } from '../../../../core/models/ticket.model';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-ticket-detail',
  standalone: false,
  templateUrl: './ticket-detail.html'
})
export class TicketDetailComponent implements OnInit {
  // Ticket Data
  ticket: Ticket | null = null;
  isLoading: boolean = true;

  // Admin Variables
  selectedStatus: string = '';
  selectedCategory: number = 0;
  adminResponseText: string = '';
  selectedFinalCategory: number = 0;

  isUpdating: boolean = false;
  userRole: string | null = null;
  currentTicketId: number = 0;

  // Chat Variables
  messages: any[] = [];
  newMessageText: string = '';
  isSendingMessage: boolean = false;
  currentUserId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private ticketService: TicketService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const decoded = this.authService.getDecodedToken() as any;
    this.userRole = decoded ? decoded.role : null;
    this.currentUserId = decoded ? Number(decoded.nameid || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decoded.sub) : 0;

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.currentTicketId = Number(idParam);

        this.loadTicket(this.currentTicketId);
        this.loadMessages(this.currentTicketId);
      } else {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  // Load Ticket Data
  loadTicket(id: number): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.ticketService.getTicketById(id).subscribe({
      next: (data) => {
        this.ticket = data;
        this.isLoading = false;
        this.cdr.detectChanges();

        this.selectedStatus = data.status || data['status'] || 'Açık';
        this.selectedFinalCategory = data.finalCategoryId || data['finalCategoryId'] || data.predictedCategoryId || data['predictedCategoryId'] || 1;
        this.adminResponseText = data.adminResponse || data['adminResponse'] || '';
      },
      error: (err) => {
        console.error('Bilet çekilemedi:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Navigate Back
  goBack(): void {
    window.history.back();
  }

  // Category Name mapping
  getCategoryName(categoryId: number | undefined): string {
    if (!categoryId) return 'Tahmin Bekleniyor';

    switch (categoryId) {
      case 1: return 'Yazılım / Uygulama';
      case 2: return 'Donanım / Arıza';
      case 3: return 'Ağ ve İnternet';
      case 4: return 'Kullanıcı İşlemleri / Şifre';
      default: return `Kategori ${categoryId}`;
    }
  }

  // Status CSS Classes
  getStatusClass(status: string): string {
    const s = status?.toLowerCase() || '';
    if (s === 'çözüldü') return 'bg-emerald-500 text-white shadow-sm';
    if (s === 'açık') return 'bg-blue-50 text-blue-700 border border-blue-200';
    if (s === 'işlemde') return 'bg-orange-50 text-orange-700 border border-orange-200';
    if (s === 'reddedildi') return 'bg-red-500 text-white shadow-sm';
    return 'bg-slate-100 text-slate-600';
  }

  // Urgency CSS Classes
  getUrgencyClass(urgency: string): string {
    const u = urgency?.toLowerCase() || '';
    if (u === 'düşük') return 'bg-green-100 text-green-700';
    if (u === 'normal' || u === 'orta') return 'bg-yellow-100 text-yellow-800';
    if (u === 'yüksek' || u === 'acil') return 'bg-red-100 text-red-700 font-bold';
    return 'bg-slate-100 text-slate-600';
  }

  // Update Ticket (Admin)
  updateTicket(): void {
    if (!this.ticket) return;

    this.isUpdating = true;

    const updateData: any = {
      ticketId: this.currentTicketId,
      status: this.selectedStatus,
      finalCategoryId: Number(this.selectedFinalCategory),
      adminResponse: this.adminResponseText
    };

    this.ticketService.updateTicket(updateData).subscribe({
      next: (res) => {
        this.ticket!.status = this.selectedStatus;
        this.ticket!.finalCategoryId = Number(this.selectedFinalCategory);
        this.ticket!.adminResponse = this.adminResponseText;

        this.isUpdating = false;
        alert("Bilet başarıyla güncellendi ve ilgili kullanıcıya bilgi iletildi!");
      },
      error: (err) => {
        this.isUpdating = false;
        console.error("Güncelleme hatası:", err);
        alert("Bilet güncellenirken bir backend tabanlı hata oluştu.");
      }
    });
  }

  // Load Chat Messages
  loadMessages(ticketId: number): void {
    this.ticketService.getTicketMessages(ticketId).subscribe({
      next: (data) => {
        this.messages = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Mesajlar yüklenemedi:', err)
    });
  }

  // Send Chat Message
  sendMessage(): void {
    if (!this.newMessageText.trim() || this.isSendingMessage) return;

    this.isSendingMessage = true;

    const messageDto = {
      ticketId: this.currentTicketId,
      messageText: this.newMessageText
    };

    this.ticketService.addTicketMessage(this.currentTicketId, messageDto).subscribe({
      next: (res) => {
        this.messages.push(res);
        this.newMessageText = '';
        this.isSendingMessage = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Mesaj gönderilemedi:', err);
        this.isSendingMessage = false;
      }
    });
  }
}