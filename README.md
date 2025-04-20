# Restaurant Management System - Backend

A comprehensive ASP.NET Core backend solution for restaurant management, featuring order processing, menu management, reservations, user authentication, and real-time notifications.

## 🍽️ Features

### User Authentication & Authorization
- Secure JWT-based authentication
- Role-based access control (Admin, Staff, Customer)
- User registration and profile management
- Password change functionality

### Menu Management
- Categories and item organization
- Detailed item properties (allergens, dietary info, calories, etc.)
- Image upload and management
- Featured items, discounts, and availability status
- Search functionality

### Order Processing
- Multi-item order creation
- Real-time order status tracking
- Order history and details
- Payment processing
- Estimated wait time calculation
- Special instructions and customizations

### Reservation System
- Table availability checking
- Reservation creation and management
- Automatic table assignment
- Status updates (confirmed, completed, canceled, no-show)
- Special requests handling

### Real-time Notifications
- SignalR-based notification system
- Order status updates
- Reservation confirmations
- Promotional messaging
- Staff alerts

### Reporting & Analytics
- Sales reporting (daily, weekly, monthly)
- Top-selling items analysis
- Sales by category
- Orders by hour visualization
- Export capabilities (CSV, JSON)
- Dashboard summary with KPIs

### Image Storage
- Cloud-based image storage with MinIO
- Local storage fallback option
- Automatic image optimization

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Language**: C# 12
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Real-time Communication**: SignalR
- **Object Mapping**: AutoMapper
- **API Documentation**: Swagger/OpenAPI
- **Object Storage**: MinIO
- **Containerization**: Docker

## 📦 Architecture

The application follows clean architecture principles with a clear separation of concerns:

- **Controllers**: Handle HTTP requests/responses
- **Services**: Implement business logic
- **Repositories**: Manage data access
- **Models**: Define domain entities
- **DTOs**: Transfer data between layers
- **Hubs**: Enable real-time communication
- **Middlewares**: Process request pipeline
- **Helpers**: Provide utility functions

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL
- Docker and Docker Compose 

### Configuration

1. Clone the repository:
   ```
   git clone https://github.com/bouba-34/restau_app_backend.git
   cd restau_app_backend
   ```

2. Configure the database connection in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=RestaurantDB;Username=postgres;Password=yourpassword;"
   }
   ```

3. Configure JWT settings in `appsettings.json`:
   ```json
   "JwtConfig": {
     "Secret": "your-secret-key-at-least-16-characters",
     "Issuer": "restaurant-api",
     "Audience": "restaurant-clients",
     "ExpirationInMinutes": 60
   }
   ```

4. Configure storage settings in `appsettings.json`:
   ```json
   "CloudStorageConfig": {
     "Endpoint": "localhost:9000",
     "AccessKey": "minioadmin",
     "SecretKey": "minioadmin",
     "BucketName": "restaurant-images",
     "UseSSL": false,
     "PublicEndpoint": "localhost:9000",
     "UseLocalStorage": false,
     "LocalStoragePath": "wwwroot/images"
   }
   ```

### Running 
1. Start the minio service using Docker Compose:
   ```
   docker-compose up -d
   ```



2. The API will be available at `http://localhost:5238`

## 📝 API Documentation

Once the application is running, you can access the Swagger documentation at:
```
http://localhost:5238/swagger
```

## 🔑 Authentication

1. Register a new user:
   ```http
   POST /api/Auth/register
   Content-Type: application/json

   {
     "username": "user1",
     "email": "user1@example.com",
     "phoneNumber": "1234567890",
     "password": "Password123!",
     "confirmPassword": "Password123!",
     "userType": "Customer"
   }
   ```

2. Login to get a JWT token:
   ```http
   POST /api/Auth/login
   Content-Type: application/json

   {
     "username": "user1",
     "password": "Password123!"
   }
   ```

3. Use the token in subsequent requests:
   ```http
   GET /api/User/profile
   Authorization: Bearer your-jwt-token
   ```

## 🌐 Key API Endpoints

### Authentication
- `POST /api/Auth/register` - Register a new user
- `POST /api/Auth/login` - Authenticate a user
- `POST /api/Auth/change-password` - Change user password
- `GET /api/Auth/profile` - Get current user profile

### Menu
- `GET /api/Menu/categories` - Get all menu categories
- `GET /api/Menu/items` - Get all menu items
- `GET /api/Menu/items/category/{categoryId}` - Get items by category
- `GET /api/Menu/items/featured` - Get featured menu items
- `GET /api/Menu/items/search` - Search for menu items

### Orders
- `POST /api/Order` - Create a new order
- `GET /api/Order/{id}` - Get order details
- `PUT /api/Order/{id}/status` - Update order status
- `POST /api/Order/{id}/payment` - Process order payment
- `GET /api/Order/active` - Get active orders

### Reservations
- `POST /api/Reservation` - Create a new reservation
- `GET /api/Reservation/{id}` - Get reservation details
- `PUT /api/Reservation/{id}` - Update reservation
- `PUT /api/Reservation/{id}/status` - Update reservation status
- `POST /api/Reservation/available-tables` - Find available tables

### Reports
- `GET /api/Report/daily/{date}` - Get daily sales report
- `GET /api/Report/top-selling` - Get top-selling items
- `GET /api/Report/dashboard-summary` - Get dashboard summary

## 🔄 Real-time Communication

The application uses SignalR for real-time communication. Connect to the hub at:

```
/restauranthub
```

Available events:
- `OrderStatusChanged` - Notifies when an order status changes
- `NewOrder` - Notifies staff of new orders
- `ReservationStatusChanged` - Notifies when a reservation status changes
- `NewReservation` - Notifies staff of new reservations
- `MenuItemAvailabilityChanged` - Notifies when item availability changes
- `Notification` - General notification system

## 📄 License

[MIT License](LICENSE)

## 👥 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [MinIO Documentation](https://docs.min.io/)

## 📧 Contact

For questions, support, or contributions, please contact:

- **Name**: Boubacar Sangare
- **Email**: boubacar34sangare@gmail.com
- **LinkedIn**: [Your LinkedIn Profile](https://www.linkedin.com/in/boubacar-sangare-7725831b9/)
- **GitHub**: [Your GitHub Profile](https://github.com/bouba-34)