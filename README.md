# ReSip - Multi-Payment E-Commerce Website

An ASP.NET Core MVC e-commerce website for eco-friendly products, supporting product browsing, shopping cart, checkout, order management, and multiple payment methods including COD, MoMo, VNPay, PayPal, and SePay.

This project was developed as a real-world e-commerce simulation with a strong focus on business workflow, checkout behavior, payment confirmation, and admin management.

## Project Information

| Item | Details |
|---|---|
| Project Name | ReSip Multi-Payment E-Commerce |
| Role | Technical Team Lead / BA-oriented Developer |
| Timeline | 01/2026 - 04/2026 |
| Location | Ho Chi Minh City, Vietnam |
| Main Domain | E-commerce, Checkout, Payment Integration |
| Repository | https://github.com/ngxuanvan/ReSip-Multi-Payment-E-Commerce |

## My Role

As a Technical Team Lead with a Business Analyst-oriented mindset, I contributed to both business analysis and technical implementation.

- Defined system scope, core modules, functional requirements, and non-functional requirements.
- Designed user flows for product browsing, cart management, checkout, payment, and order history.
- Coordinated implementation across frontend, backend, database, admin, and payment modules.
- Implemented and reviewed core checkout/payment flows using ASP.NET Core MVC and Entity Framework Core.
- Integrated sandbox payment flows for MoMo, VNPay, PayPal, and SePay.
- Tested payment return, IPN, webhook, order status, and transaction logging scenarios.

## Business Context

ReSip is an e-commerce website for selling eco-friendly products such as reusable bottles and straws. The system allows customers to browse products, add items to cart, place orders, and select different payment methods.

The admin side helps the business manage products, categories, orders, users, website settings, blogs, FAQs, and payment transaction records.

## Business Problem

Customers need a simple and reliable checkout process with multiple payment options. The business needs a centralized admin system to manage products, monitor orders, track payment transactions, and support reconciliation when payment data does not match order data.

## Key Features

| Module | Description |
|---|---|
| Product Catalog | Browse products, view product details, filter by category |
| Shopping Cart | Add, update, and remove cart items |
| Checkout | Collect customer information, shipping address, and payment method |
| Multi-Payment | Supports COD, MoMo, VNPay, PayPal, and SePay |
| Order Management | Create orders, track order status, and view order history |
| Payment Confirmation | Handle return, IPN, webhook, and payment validation |
| Admin Dashboard | Manage products, categories, orders, users, blogs, FAQs, and settings |
| Payment Logging | Store transaction records for MoMo, VNPay, PayPal, and SePay |
| Email Notification | Send confirmation emails to customers and admin |

## Stakeholders

| Stakeholder | Goals |
|---|---|
| Customer | Browse products, add to cart, checkout, pay, and track orders |
| Admin | Manage catalog, orders, users, content, and payment transactions |
| Payment Gateway | Process online payments and return payment results |
| System | Validate data, update order status, deduct stock, clear cart, and send email |

## Business Rules

- Users must log in before adding products to cart or placing orders.
- COD orders are created with `ChoXuLy` status.
- Online payment orders are created with `ChoThanhToan` status.
- The system finalizes an online order only after successful payment confirmation.
- Finalizing an order includes creating order details, deducting stock, clearing the cart, and sending confirmation emails.
- If the paid amount does not match the order amount, the order is marked for reconciliation.
- Payment callbacks and webhooks are handled idempotently to reduce duplicate transaction processing.

## Checkout and Payment Flow

```mermaid
flowchart TD
    A[Customer browses products] --> B[Add product to cart]
    B --> C[View shopping cart]
    C --> D[Checkout]
    D --> E[Enter shipping information]
    E --> F{Select payment method}
    F -->|COD| G[Create order: ChoXuLy]
    F -->|MoMo / VNPay / PayPal / SePay| H[Create order: ChoThanhToan]
    H --> I[Redirect or display payment instruction]
    I --> J{Payment result}
    J -->|Success| K[Finalize order]
    J -->|Failed| L[Mark payment as failed]
    J -->|Mismatch| M[Mark order for reconciliation]
    K --> N[Create order details]
    N --> O[Deduct product stock]
    O --> P[Clear user cart]
    P --> Q[Send confirmation emails]
```

## Data Flow Overview

```mermaid
flowchart LR
    Customer[Customer] --> Views[Razor Views]
    Views --> Controllers[ASP.NET Core MVC Controllers]
    Controllers --> Services[Business and Payment Services]
    Services --> Database[(SQL Server)]
    Services --> Gateways[Payment Gateways]
    Gateways --> Callbacks[Return / IPN / Webhook]
    Callbacks --> Services
    Services --> Email[Email Service]
```

## Sequence Diagrams

### COD Checkout Sequence

```mermaid
sequenceDiagram
    actor Customer
    participant View as Razor View
    participant Checkout as CheckoutController
    participant OrderSvc as OrderService
    participant DB as SQL Server
    participant Email as Email Service

    Customer->>View: Submit checkout form with COD
    View->>Checkout: POST /Checkout/PlaceOrder
    Checkout->>DB: Get cart items and product prices
    Checkout->>DB: Create DonHang with ChoXuLy status
    Checkout->>OrderSvc: Finalize order with ChoXuLy
    OrderSvc->>DB: Create ChiTietDonHang records
    OrderSvc->>DB: Deduct product stock
    OrderSvc->>DB: Clear user cart
    OrderSvc->>Email: Send confirmation email
    Checkout-->>Customer: Redirect to order success page
```

### Online Payment Sequence

```mermaid
sequenceDiagram
    actor Customer
    participant View as Razor View
    participant Checkout as CheckoutController
    participant Gateway as Payment Gateway
    participant OrderSvc as OrderService
    participant DB as SQL Server
    participant Email as Email Service

    Customer->>View: Submit checkout form
    View->>Checkout: POST /Checkout/PlaceOrder
    Checkout->>DB: Get cart items and calculate total amount
    Checkout->>DB: Create DonHang with ChoThanhToan status
    Checkout->>Gateway: Create payment request
    Gateway-->>Customer: Redirect to payment page / QR payment
    Gateway-->>Checkout: Return / IPN / Webhook payment result
    Checkout->>Checkout: Verify signature and paid amount

    alt Payment is valid
        Checkout->>OrderSvc: Finalize order
        OrderSvc->>DB: Create order details
        OrderSvc->>DB: Deduct stock and clear cart
        OrderSvc->>Email: Send confirmation email
        Checkout-->>Customer: Show payment success
    else Payment failed
        Checkout->>DB: Update order as ThanhToanThatBai
        Checkout-->>Customer: Show payment failed
    else Amount or method mismatch
        Checkout->>DB: Update order as ThanhToanCanDoiSoat
        Checkout-->>Customer: Show reconciliation status
    end
```

### Admin Order Management Sequence

```mermaid
sequenceDiagram
    actor Admin
    participant AdminView as Admin Razor View
    participant AdminCtrl as Admin DonHangsController
    participant DB as SQL Server
    participant Cache as Memory Cache

    Admin->>AdminView: Open order management page
    AdminView->>AdminCtrl: GET /Admin/DonHangs
    AdminCtrl->>DB: Load order list
    DB-->>AdminCtrl: Return orders
    AdminCtrl-->>AdminView: Render order table

    Admin->>AdminView: Update order information or status
    AdminView->>AdminCtrl: POST /Admin/DonHangs/Edit
    AdminCtrl->>DB: Update DonHang record
    AdminCtrl->>Cache: Clear dashboard statistics cache
    AdminCtrl-->>AdminView: Redirect to order list
```

## Order Status Flow

```mermaid
stateDiagram-v2
    [*] --> ChoXuLy: COD order
    [*] --> ChoThanhToan: Online payment order
    ChoThanhToan --> DaThanhToan: Payment success
    ChoThanhToan --> ThanhToanThatBai: Payment failed
    ChoThanhToan --> ThanhToanCanDoiSoat: Amount or method mismatch
    ChoThanhToan --> HetHan: Payment expired
    ChoXuLy --> HoanThanh: Admin completes order
    DaThanhToan --> HoanThanh: Admin completes order
```

## Simplified ERD

```mermaid
erDiagram
    User ||--o{ GioHang : owns
    User ||--o{ DonHang : places
    Category ||--o{ SanPham : groups
    SanPham ||--o{ GioHang : added_to
    SanPham ||--o{ ChiTietDonHang : ordered_as
    DonHang ||--o{ ChiTietDonHang : contains
    DonHang ||--o{ MomoTransaction : maps
    DonHang ||--o{ VnPayTransaction : maps
    DonHang ||--o{ PayPalTransaction : maps
    DonHang ||--o{ SePayTransaction : maps
```

## Requirement Traceability

| Business Need | Feature | Main Module | Status |
|---|---|---|---|
| Customers can browse products | Product listing and details | `SanPhamController` | Done |
| Customers can buy multiple items | Shopping cart | `CartController` | Done |
| Customers can place orders | Checkout | `CheckoutController` | Done |
| Customers can use multiple payment methods | COD, MoMo, VNPay, PayPal, SePay | Payment services | Done |
| Admin can manage orders | Order management | `Areas/Admin/DonHangsController` | Done |
| Business can reconcile payments | Transaction logging | Payment transaction models | Done |
| System can notify users | Email confirmation | `OrderService`, `OrderEmailBuilder` | Done |

## Technical Highlights

- Built with ASP.NET Core MVC and Entity Framework Core.
- Designed a complete checkout flow from cart to payment confirmation.
- Integrated multiple payment gateways in sandbox mode.
- Implemented payment return, IPN, and webhook handling.
- Added amount validation before finalizing online payment orders.
- Stored gateway transaction data for payment tracking and reconciliation.
- Used cookie-based authentication for customer and admin access.
- Applied admin area routing for back-office management.
- Used Serilog for application logging.

## Tech Stack

| Layer | Technologies |
|---|---|
| Backend | ASP.NET Core MVC, C# |
| Database | SQL Server, Entity Framework Core |
| Frontend | Razor Views, HTML, CSS, Bootstrap, JavaScript, jQuery |
| Authentication | Cookie Authentication |
| Payment | MoMo, VNPay, PayPal, SePay |
| Email | SMTP / MailKit |
| Logging | Serilog |

## Screenshots

Add the following images later under `docs/screenshots/`.

| Screen | Image File |
|---|---|
| Home Page | `docs/screenshots/01-home-page.png` |
| Product Listing | `docs/screenshots/02-product-listing.png` |
| Product Detail | `docs/screenshots/03-product-detail.png` |
| Shopping Cart | `docs/screenshots/04-shopping-cart.png` |
| Checkout Page | `docs/screenshots/05-checkout-page.png` |
| MoMo Payment | `docs/screenshots/06-momo-payment.png` |
| VNPay Payment | `docs/screenshots/07-vnpay-payment.png` |
| PayPal Payment | `docs/screenshots/08-paypal-payment.png` |
| SePay QR Payment | `docs/screenshots/09-sepay-payment.png` |
| Order Success | `docs/screenshots/10-order-success.png` |
| Admin Dashboard | `docs/screenshots/11-admin-dashboard.png` |
| Admin Order Management | `docs/screenshots/12-admin-orders.png` |
| Admin Product Management | `docs/screenshots/13-admin-products.png` |
| Payment Transaction Management | `docs/screenshots/14-payment-transactions.png` |

### Preview Placeholders

![Home Page](docs/screenshots/01-home-page.png)
![Checkout Page](docs/screenshots/05-checkout-page.png)
![Admin Order Management](docs/screenshots/12-admin-orders.png)

## Folder Structure

```text
ResipWeb/
├── Areas/Admin/              # Admin controllers and views
├── Controllers/              # Customer-facing MVC controllers
├── Models/                   # Entity models and view models
├── Services/                 # Order, email, exchange rate, and payment services
├── Views/                    # Razor views
├── wwwroot/                  # Static assets and uploaded images
├── Migrations/               # EF Core migrations
└── Database/                 # Database backup file
```

## Setup Guide

1. Restore the SQL Server database from `Database/ResipWebDb.bak`.
2. Update the connection string in `appsettings.json`.
3. Configure sandbox credentials for MoMo, VNPay, PayPal, and SePay.
4. Configure email settings for SMTP.
5. Run the project:

```bash
dotnet run
```

## BA Analysis Highlights

- Identified key actors, business goals, and system boundaries.
- Designed user flows for product browsing, cart, checkout, and payment confirmation.
- Defined order status transitions for COD and online payment scenarios.
- Added reconciliation handling for payment amount mismatch cases.
- Designed admin workflows for managing catalog, orders, and transactions.
- Considered real-world payment risks such as duplicate callback, failed payment, and delayed webhook.

## Future Improvements

- Add automated unit and integration tests for checkout and payment flows.
- Improve order status standardization across customer and admin dashboards.
- Add stock validation before checkout confirmation.
- Add advanced search and filtering for admin order management.
- Add customer-facing order tracking by order code.
- Add deployment guide and live demo video.
