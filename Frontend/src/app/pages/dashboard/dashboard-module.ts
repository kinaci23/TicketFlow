import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms'
import { RouterModule } from '@angular/router';
import { DashboardRoutingModule } from './dashboard-routing-module';

// Components
import { SidebarComponent } from './layout/sidebar/sidebar';
import { TopbarComponent } from './layout/topbar/topbar';
import { LayoutComponent } from './layout/layout';
import { TicketListComponent } from './tickets/ticket-list/ticket.list';
import { TicketDetailComponent } from './tickets/ticket-detail/ticket-detail';
import { TicketCreateComponent } from './tickets/ticket-create/ticket-create';

@NgModule({
  declarations: [
    SidebarComponent,
    TopbarComponent,
    LayoutComponent,
    TicketListComponent,
    TicketDetailComponent,
    TicketCreateComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    DashboardRoutingModule,
    ReactiveFormsModule,
    FormsModule
  ],
  exports: [
    SidebarComponent
  ]
})
export class DashboardModule { }