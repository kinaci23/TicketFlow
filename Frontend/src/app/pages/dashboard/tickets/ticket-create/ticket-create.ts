import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TicketService } from '../../../../core/services/ticket.service';
import { TicketCreateDto } from '../../../../core/models/ticket.model';

@Component({
  selector: 'app-ticket-create',
  standalone: false,
  templateUrl: './ticket-create.html'
})
export class TicketCreateComponent implements OnInit {
  // Form Variables
  ticketForm!: FormGroup;
  isSubmitting: boolean = false;

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.ticketForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      urgency: ['Normal', Validators.required]
    });
  }

  // Submit Ticket creation
  onSubmit(): void {
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const newTicket: TicketCreateDto = {
      title: this.ticketForm.value.title,
      description: this.ticketForm.value.description,
      urgency: this.ticketForm.value.urgency
    };

    console.log("Yapay Zeka'ya ve Backend'e kargo çıkıyor...", newTicket);

    this.ticketService.createTicket(newTicket).subscribe({
      next: (res) => {
        console.log("Bilet başarıyla oluşturuldu!", res);
        this.isSubmitting = false;

        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error("Bilet oluşturulurken hata:", err);
        alert("Bilet oluşturulamadı. Lütfen sunucunun (Backend) çalıştığından emin olun.");
        this.isSubmitting = false;
      }
    });
  }

  // Go Back
  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}