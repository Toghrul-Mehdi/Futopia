🏟️ Futopia – Football Stadium Rental Platform

Futopia is a modern and user-friendly platform that allows players, teams, and organizations to rent football stadiums quickly and efficiently. The platform connects stadium owners with users through a clean interface, providing availability schedules, booking options, secure payments, and real-time management tools.

🚀 Features
🔐 Authentication & User Roles

JWT-based authentication

Email verification (via JWT link)

Password reset & account recovery

User roles: SuperAdmin, Admin, User, Support,Staff

🏟️ Stadium Management

Add, edit, or delete stadium listings

Upload images, pricing, field size, turf type, and amenities

Set availability schedule (daily/hourly)

Manage bookings from dashboard

📅 Booking System

Real-time availability check

Fast reservation in a few clicks

Cancel or reschedule bookings

Booking status tracking

Notification system (email/SMS)

💳 Payments

Integration with Kapital Bank payment gateway

Secure card payments

Automatic transaction logging

Refund & cancellation policy support

🖥️ Admin Panel

Manage all users and stadiums

View statistics and analytics

Approve or decline new stadium submissions

Monitor payments and system logs

🏗️ Architecture

The platform is built using Onion Architecture + CQRS:

Frontend: React

Backend: ASP.NET Web API

Application Layer: CQRS, Mediator, FluentValidation

Domain Layer: Entities, business rules, domain events

Infrastructure: EF Core, Kapital Bank integration, Infobip SMS/Email
