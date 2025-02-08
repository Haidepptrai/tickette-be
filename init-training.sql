-- Drop existing tables to prevent conflicts
DROP TABLE IF EXISTS UserBuyHistory;
DROP TABLE IF EXISTS Tickets;
DROP TABLE IF EXISTS EventDates;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS Users;

-- Users Table
CREATE TABLE Users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL
);

-- Events Table
CREATE TABLE Events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    location VARCHAR(255) NOT NULL,
    city VARCHAR(100) NOT NULL,
    district VARCHAR(100) NOT NULL,
    ward VARCHAR(100) NOT NULL,
    street_address VARCHAR(255) NOT NULL,
    category_id UUID NOT NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL
);

-- Event Dates Table
CREATE TABLE EventDates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id UUID REFERENCES Events(id) ON DELETE CASCADE,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL
);

-- Tickets Table
CREATE TABLE Tickets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_date_id UUID REFERENCES EventDates(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    total_tickets INT NOT NULL,
    min_per_order INT NOT NULL,
    max_per_order INT NOT NULL,
    sale_start TIMESTAMP NOT NULL,
    sale_end TIMESTAMP NOT NULL,
    is_free BOOLEAN DEFAULT FALSE
);

-- UserBuyHistory Table
CREATE TABLE UserBuyHistory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES Users(id) ON DELETE CASCADE,
    event_id UUID REFERENCES Events(id) ON DELETE CASCADE,
    ticket_id UUID REFERENCES Tickets(id) ON DELETE CASCADE,
    purchase_date TIMESTAMP DEFAULT NOW()
);

-- Insert Users (10 users)
INSERT INTO Users (id, name, email) VALUES
    (gen_random_uuid(), 'Alice', 'alice@example.com'),
    (gen_random_uuid(), 'Bob', 'bob@example.com'),
    (gen_random_uuid(), 'Charlie', 'charlie@example.com'),
    (gen_random_uuid(), 'David', 'david@example.com'),
    (gen_random_uuid(), 'Eve', 'eve@example.com'),
    (gen_random_uuid(), 'Frank', 'frank@example.com'),
    (gen_random_uuid(), 'Grace', 'grace@example.com'),
    (gen_random_uuid(), 'Hank', 'hank@example.com'),
    (gen_random_uuid(), 'Ivy', 'ivy@example.com'),
    (gen_random_uuid(), 'Jack', 'jack@example.com');

-- Insert Events (10 events)
INSERT INTO Events (id, name, location, city, district, ward, street_address, category_id, start_date, end_date) VALUES
    (gen_random_uuid(), 'Summer Music Festival', 'Venue A', 'Ho Chi Minh', 'District 1', 'Ward 1', '123 Street A', gen_random_uuid(), '2025-06-20 18:00:00', '2025-06-23 23:59:59'),
    (gen_random_uuid(), 'Tech Expo 2025', 'Venue B', 'Ho Chi Minh', 'District 7', 'Ward 2', '456 Tech Street', gen_random_uuid(), '2025-07-10 10:00:00', '2025-07-12 18:00:00'),
    (gen_random_uuid(), 'Gaming Con', 'Venue C', 'Ho Chi Minh', 'District 3', 'Ward 5', '789 Gamer Road', gen_random_uuid(), '2025-08-15 09:00:00', '2025-08-17 20:00:00'),
    (gen_random_uuid(), 'Art Exhibition', 'Venue D', 'Hanoi', 'Ba Dinh', 'Ward 10', '111 Art Lane', gen_random_uuid(), '2025-09-01 10:00:00', '2025-09-05 18:00:00'),
    (gen_random_uuid(), 'Food Fest', 'Venue E', 'Da Nang', 'Hai Chau', 'Ward 3', '222 Food Plaza', gen_random_uuid(), '2025-10-15 12:00:00', '2025-10-17 23:00:00'),
    (gen_random_uuid(), 'Business Summit', 'Venue F', 'Ho Chi Minh', 'District 4', 'Ward 6', '333 Summit Blvd', gen_random_uuid(), '2025-11-20 08:00:00', '2025-11-22 18:00:00'),
    (gen_random_uuid(), 'Fashion Show', 'Venue G', 'Hanoi', 'Hoan Kiem', 'Ward 7', '444 Fashion Street', gen_random_uuid(), '2025-12-05 19:00:00', '2025-12-06 22:00:00'),
    (gen_random_uuid(), 'Movie Premiere', 'Venue H', 'Ho Chi Minh', 'District 9', 'Ward 8', '555 Cinema Road', gen_random_uuid(), '2025-12-20 18:00:00', '2025-12-21 22:00:00'),
    (gen_random_uuid(), 'Charity Concert', 'Venue I', 'Da Nang', 'Son Tra', 'Ward 9', '666 Charity Lane', gen_random_uuid(), '2026-01-10 17:00:00', '2026-01-10 22:00:00'),
    (gen_random_uuid(), 'E-Sports Championship', 'Venue J', 'Ho Chi Minh', 'District 10', 'Ward 11', '777 E-Sports Arena', gen_random_uuid(), '2026-02-01 10:00:00', '2026-02-03 20:00:00');

-- Insert Event Dates (10 event dates)
INSERT INTO EventDates (id, event_id, start_date, end_date) VALUES
    (gen_random_uuid(), (SELECT id FROM Events ORDER BY RANDOM() LIMIT 1), '2025-06-20 18:00:00', '2025-06-20 23:59:59'),
    (gen_random_uuid(), (SELECT id FROM Events ORDER BY RANDOM() LIMIT 1), '2025-07-10 10:00:00', '2025-07-10 18:00:00'),
    (gen_random_uuid(), (SELECT id FROM Events ORDER BY RANDOM() LIMIT 1), '2025-08-15 09:00:00', '2025-08-15 20:00:00');

-- Insert Tickets (10 tickets)
INSERT INTO Tickets (id, event_date_id, name, price, total_tickets, min_per_order, max_per_order, sale_start, sale_end, is_free) VALUES
    (gen_random_uuid(), (SELECT id FROM EventDates ORDER BY RANDOM() LIMIT 1), 'General Admission', 50.00, 200, 1, 4, '2025-02-03 00:00:00', '2025-02-25 23:59:59', FALSE),
    (gen_random_uuid(), (SELECT id FROM EventDates ORDER BY RANDOM() LIMIT 1), 'VIP', 100.00, 50, 1, 2, '2025-02-03 00:00:00', '2025-02-25 23:59:59', FALSE);

-- Insert UserBuyHistory (10 purchases)
INSERT INTO UserBuyHistory (id, user_id, event_id, ticket_id, purchase_date) VALUES
    (gen_random_uuid(), (SELECT id FROM Users ORDER BY RANDOM() LIMIT 1), (SELECT id FROM Events ORDER BY RANDOM() LIMIT 1), (SELECT id FROM Tickets ORDER BY RANDOM() LIMIT 1), NOW()),
    (gen_random_uuid(), (SELECT id FROM Users ORDER BY RANDOM() LIMIT 1), (SELECT id FROM Events ORDER BY RANDOM() LIMIT 1), (SELECT id FROM Tickets ORDER BY RANDOM() LIMIT 1), NOW());
