-- Training Database Schema for Recommendation System
-- Core Tables (Mirror of Production)

CREATE TABLE users (
    id UUID PRIMARY KEY,
    user_name VARCHAR(256) NULL,
    full_name VARCHAR(100) NULL,
    gender SMALLINT NOT NULL DEFAULT 0
);

CREATE TABLE events (
    id UUID PRIMARY KEY,
    category_id UUID NOT NULL,
    created_by_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    location_name VARCHAR(255) NOT NULL,
    city VARCHAR(100) NOT NULL,
    district VARCHAR(100) NOT NULL,
    ward VARCHAR(100) NOT NULL,
    street_address VARCHAR(255) NOT NULL,
    description TEXT NULL,
    banner VARCHAR(255) NULL,
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    event_slug VARCHAR(300) NOT NULL,
    status SMALLINT NOT NULL
);

CREATE TABLE categories (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE orders (
    id UUID PRIMARY KEY,
    event_id UUID NOT NULL,
    user_ordered_id UUID NOT NULL,
    total_price DECIMAL(18,2) NOT NULL,
    total_quantity INT NOT NULL,
    created_at TIMESTAMP NOT NULL
);

-- Interaction & Training Data Tables

CREATE TABLE user_event_interactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    event_id UUID NOT NULL,
    interaction_type VARCHAR(50) NOT NULL,
    interaction_strength FLOAT NOT NULL,
    interaction_count INT NOT NULL DEFAULT 1,
    last_interaction_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_user_event_interactions_user ON user_event_interactions(user_id);
CREATE INDEX ix_user_event_interactions_event ON user_event_interactions(event_id);
CREATE INDEX ix_user_event_interactions_type ON user_event_interactions(interaction_type);
CREATE UNIQUE INDEX uq_user_event_interaction ON user_event_interactions(user_id, event_id, interaction_type);

CREATE TABLE event_features (
    event_id UUID PRIMARY KEY,
    popularity FLOAT NOT NULL DEFAULT 0,
    avg_ticket_price DECIMAL(18,2) NULL,
    view_count INT NOT NULL DEFAULT 0,
    purchase_count INT NOT NULL DEFAULT 0,
    predicted_demand FLOAT NULL,
    last_updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_features (
    user_id UUID PRIMARY KEY,
    activity_level FLOAT NOT NULL DEFAULT 0,
    price_sensitivity FLOAT NULL,
    event_category_preferences TEXT NULL,
    last_updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE training_datasets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sample_count INT NOT NULL,
    dataset_path VARCHAR(255) NULL,
    dataset_json TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE model_registry (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    model_name VARCHAR(100) NOT NULL,
    model_version VARCHAR(20) NOT NULL,
    training_dataset_id UUID NULL,
    model_path VARCHAR(255) NOT NULL,
    trained_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metrics TEXT NOT NULL,
    parameters TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT fk_model_registry_training_dataset 
        FOREIGN KEY (training_dataset_id) 
        REFERENCES training_datasets(id)
);

-- Data Sync and ETL Stored Procedures

-- Note: Replace [ProductionDB] with your actual production database name in these procedures

CREATE OR REPLACE FUNCTION sync_users() 
RETURNS VOID AS $$
BEGIN
    -- For demonstration purposes, using a temporary table structure to simulate the MERGE
    -- In a real implementation, you would query from your production database
    CREATE TEMPORARY TABLE temp_users (
        id UUID,
        user_name VARCHAR(256),
        full_name VARCHAR(100),
        gender SMALLINT
    );
    
    -- In a real scenario, this would be populated with data from your production database
    -- INSERT INTO temp_users 
    -- SELECT id, user_name, full_name, gender FROM production_database.users;
    
    -- Update existing users
    UPDATE users
    SET user_name = tu.user_name,
        full_name = tu.full_name,
        gender = tu.gender
    FROM temp_users tu
    WHERE users.id = tu.id;
    
    -- Insert new users
    INSERT INTO users (id, user_name, full_name, gender)
    SELECT tu.id, tu.user_name, tu.full_name, tu.gender
    FROM temp_users tu
    WHERE NOT EXISTS (SELECT 1 FROM users u WHERE u.id = tu.id);
    
    -- Clean up
    DROP TABLE IF EXISTS temp_users;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sync_events() 
RETURNS VOID AS $$
BEGIN
    -- For demonstration purposes, using a temporary table
    CREATE TEMPORARY TABLE temp_events (
        id UUID,
        category_id UUID,
        created_by_id UUID,
        name VARCHAR(255),
        location_name VARCHAR(255),
        city VARCHAR(100),
        district VARCHAR(100),
        ward VARCHAR(100),
        street_address VARCHAR(255),
        description TEXT,
        banner VARCHAR(255),
        start_date TIMESTAMP,
        end_date TIMESTAMP,
        event_slug VARCHAR(300),
        status SMALLINT
    );
    
    -- In a real scenario, this would be populated with data from your production database
    -- INSERT INTO temp_events 
    -- SELECT * FROM production_database.events;
    
    -- Update existing events
    UPDATE events
    SET category_id = te.category_id,
        name = te.name,
        location_name = te.location_name,
        city = te.city,
        district = te.district,
        ward = te.ward,
        street_address = te.street_address,
        description = te.description,
        banner = te.banner,
        start_date = te.start_date,
        end_date = te.end_date,
        event_slug = te.event_slug,
        status = te.status
    FROM temp_events te
    WHERE events.id = te.id;
    
    -- Insert new events
    INSERT INTO events (id, category_id, created_by_id, name, location_name, city, district,
                      ward, street_address, description, banner, start_date, end_date,
                      event_slug, status)
    SELECT te.id, te.category_id, te.created_by_id, te.name, te.location_name, te.city, te.district,
           te.ward, te.street_address, te.description, te.banner, te.start_date, te.end_date,
           te.event_slug, te.status
    FROM temp_events te
    WHERE NOT EXISTS (SELECT 1 FROM events e WHERE e.id = te.id);
    
    -- Clean up
    DROP TABLE IF EXISTS temp_events;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sync_categories() 
RETURNS VOID AS $$
BEGIN
    -- For demonstration purposes
    CREATE TEMPORARY TABLE temp_categories (
        id UUID,
        name VARCHAR(100)
    );
    
    -- In a real scenario, populate with production data
    -- INSERT INTO temp_categories
    -- SELECT id, name FROM production_database.categories;
    
    -- Update existing categories
    UPDATE categories
    SET name = tc.name
    FROM temp_categories tc
    WHERE categories.id = tc.id;
    
    -- Insert new categories
    INSERT INTO categories (id, name)
    SELECT tc.id, tc.name
    FROM temp_categories tc
    WHERE NOT EXISTS (SELECT 1 FROM categories c WHERE c.id = tc.id);
    
    -- Clean up
    DROP TABLE IF EXISTS temp_categories;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sync_orders() 
RETURNS VOID AS $$
BEGIN
    -- For demonstration purposes
    CREATE TEMPORARY TABLE temp_orders (
        id UUID,
        event_id UUID,
        user_ordered_id UUID,
        total_price DECIMAL(18,2),
        total_quantity INT,
        created_at TIMESTAMP
    );
    
    -- In a real scenario, populate with recent production data
    -- INSERT INTO temp_orders
    -- SELECT id, event_id, user_ordered_id, total_price, total_quantity, created_at
    -- FROM production_database.orders
    -- WHERE created_at > (CURRENT_TIMESTAMP - INTERVAL '90 days');
    
    -- Update existing orders
    UPDATE orders
    SET total_price = to_orders.total_price,
        total_quantity = to_orders.total_quantity
    FROM temp_orders to_orders
    WHERE orders.id = to_orders.id;
    
    -- Insert new orders
    INSERT INTO orders (id, event_id, user_ordered_id, total_price, total_quantity, created_at)
    SELECT to_orders.id, to_orders.event_id, to_orders.user_ordered_id, 
           to_orders.total_price, to_orders.total_quantity, to_orders.created_at
    FROM temp_orders to_orders
    WHERE NOT EXISTS (SELECT 1 FROM orders o WHERE o.id = to_orders.id);
    
    -- Generate purchase interactions from orders
    INSERT INTO user_event_interactions (user_id, event_id, interaction_type, interaction_strength, interaction_count, last_interaction_at)
    SELECT 
        o.user_ordered_id, o.event_id, 'Purchase', 1.0, 1, o.created_at
    FROM orders o
    WHERE NOT EXISTS (
        SELECT 1 FROM user_event_interactions uei 
        WHERE uei.user_id = o.user_ordered_id AND uei.event_id = o.event_id AND uei.interaction_type = 'Purchase'
    );
    
    -- Clean up
    DROP TABLE IF EXISTS temp_orders;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sync_interactions() 
RETURNS VOID AS $$
BEGIN
    -- For demonstration purposes
    CREATE TEMPORARY TABLE temp_page_views (
        user_id UUID,
        event_id UUID,
        duration_seconds INT,
        viewed_at TIMESTAMP
    );
    
    -- Then create a temporary table with processed view data:
    CREATE TEMPORARY TABLE temp_interactions AS
    SELECT 
        user_id, 
        event_id, 
        'View' AS interaction_type,
        CASE 
            WHEN duration_seconds > 300 THEN 0.7 -- Long view (>5 min)
            WHEN duration_seconds > 60 THEN 0.5  -- Medium view (1-5 min)
            ELSE 0.3                           -- Brief view (<1 min)
        END AS interaction_strength,
        COUNT(*) AS interaction_count,
        MAX(viewed_at) AS last_interaction_at
    FROM temp_page_views
    WHERE viewed_at > (CURRENT_TIMESTAMP - INTERVAL '30 days')
    AND user_id IS NOT NULL
    GROUP BY user_id, event_id, 
        CASE 
            WHEN duration_seconds > 300 THEN 0.7
            WHEN duration_seconds > 60 THEN 0.5
            ELSE 0.3
        END;
    
    -- Update existing interactions
    UPDATE user_event_interactions
    SET interaction_count = ti.interaction_count,
        interaction_strength = ti.interaction_strength,
        last_interaction_at = ti.last_interaction_at
    FROM temp_interactions ti
    WHERE user_event_interactions.user_id = ti.user_id
    AND user_event_interactions.event_id = ti.event_id
    AND user_event_interactions.interaction_type = ti.interaction_type;
    
    -- Insert new interactions
    INSERT INTO user_event_interactions 
        (user_id, event_id, interaction_type, interaction_strength, interaction_count, last_interaction_at)
    SELECT 
        ti.user_id, ti.event_id, ti.interaction_type, 
        ti.interaction_strength, ti.interaction_count, ti.last_interaction_at
    FROM temp_interactions ti
    WHERE NOT EXISTS (
        SELECT 1 FROM user_event_interactions uei 
        WHERE uei.user_id = ti.user_id 
        AND uei.event_id = ti.event_id 
        AND uei.interaction_type = ti.interaction_type
    );
    
    -- Clean up
    DROP TABLE IF EXISTS temp_page_views;
    DROP TABLE IF EXISTS temp_interactions;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_event_features() 
RETURNS VOID AS $$
BEGIN
    -- Create a temporary table with all the data we need
    CREATE TEMPORARY TABLE temp_event_features AS
    SELECT 
        e.id AS event_id,
        COALESCE(ViewCounts.view_count, 0) AS view_count,
        COALESCE(PurchaseCounts.purchase_count, 0) AS purchase_count,
        COALESCE(AvgPrices.avg_price, 0) AS avg_ticket_price,
        -- Calculate popularity score (normalized to 0-1)
        CASE 
            WHEN MAX(AllCounts.total_interactions) OVER() = 0 THEN 0
            ELSE CAST(COALESCE(AllCounts.total_interactions, 0) AS FLOAT) / 
                NULLIF(MAX(AllCounts.total_interactions) OVER(), 0)
        END AS popularity
    FROM events e
    LEFT JOIN (
        -- View counts per event
        SELECT event_id, SUM(interaction_count) AS view_count
        FROM user_event_interactions
        WHERE interaction_type = 'View'
        GROUP BY event_id
    ) ViewCounts ON e.id = ViewCounts.event_id
    LEFT JOIN (
        -- Purchase counts per event
        SELECT event_id, SUM(interaction_count) AS purchase_count
        FROM user_event_interactions
        WHERE interaction_type = 'Purchase'
        GROUP BY event_id
    ) PurchaseCounts ON e.id = PurchaseCounts.event_id
    LEFT JOIN (
        -- Average ticket price
        SELECT event_id, AVG(total_price / total_quantity) AS avg_price
        FROM orders
        GROUP BY event_id
    ) AvgPrices ON e.id = AvgPrices.event_id
    LEFT JOIN (
        -- Total interactions per event (for normalization)
        SELECT event_id, SUM(interaction_count) AS total_interactions
        FROM user_event_interactions
        GROUP BY event_id
    ) AllCounts ON e.id = AllCounts.event_id;
    
    -- Update existing features
    UPDATE event_features
    SET view_count = tef.view_count,
        purchase_count = tef.purchase_count,
        avg_ticket_price = tef.avg_ticket_price,
        popularity = tef.popularity,
        last_updated_at = CURRENT_TIMESTAMP
    FROM temp_event_features tef
    WHERE event_features.event_id = tef.event_id;
    
    -- Insert new features
    INSERT INTO event_features 
        (event_id, view_count, purchase_count, avg_ticket_price, popularity, last_updated_at)
    SELECT 
        tef.event_id, tef.view_count, tef.purchase_count, 
        tef.avg_ticket_price, tef.popularity, CURRENT_TIMESTAMP
    FROM temp_event_features tef
    WHERE NOT EXISTS (
        SELECT 1 FROM event_features ef WHERE ef.event_id = tef.event_id
    );
    
    -- Clean up
    DROP TABLE IF EXISTS temp_event_features;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_user_features() 
RETURNS VOID AS $$
BEGIN
    -- Create a temporary table with all the data we need
    CREATE TEMPORARY TABLE temp_user_features AS
    SELECT 
        u.id AS user_id,
        -- Calculate activity level (normalized to 0-1)
        CASE 
            WHEN MAX(UserCounts.total_interactions) OVER() = 0 THEN 0
            ELSE CAST(COALESCE(UserCounts.total_interactions, 0) AS FLOAT) / 
                NULLIF(MAX(UserCounts.total_interactions) OVER(), 0)
        END AS activity_level,
        -- Calculate price sensitivity (based on purchase history)
        CASE 
            WHEN PriceData.avg_spend IS NULL THEN NULL
            WHEN MAX(PriceData.avg_spend) OVER() = 0 THEN 0
            ELSE 1 - (CAST(PriceData.avg_spend AS FLOAT) / 
                    NULLIF(MAX(PriceData.avg_spend) OVER(), 0))
        END AS price_sensitivity,
        -- JSON with category preferences (PostgreSQL uses json_agg instead of FOR JSON PATH)
        (
            SELECT json_agg(json_build_object('category_id', cp.category_id, 'preference_score', cp.preference_score))
            FROM (
                SELECT 
                    e.category_id,
                    SUM(uei.interaction_strength * uei.interaction_count) / 
                        NULLIF(SUM(uei.interaction_count), 0) AS preference_score
                FROM user_event_interactions uei
                JOIN events e ON uei.event_id = e.id
                WHERE uei.user_id = u.id
                GROUP BY e.category_id
            ) cp
        ) AS event_category_preferences
    FROM users u
    LEFT JOIN (
        -- Total interactions per user (for normalization)
        SELECT user_id, SUM(interaction_count) AS total_interactions
        FROM user_event_interactions
        GROUP BY user_id
    ) UserCounts ON u.id = UserCounts.user_id
    LEFT JOIN (
        -- Average spend per user
        SELECT user_ordered_id, AVG(total_price) AS avg_spend
        FROM orders
        GROUP BY user_ordered_id
    ) PriceData ON u.id = PriceData.user_ordered_id;
    
    -- Update existing features
    UPDATE user_features
    SET activity_level = tuf.activity_level,
        price_sensitivity = tuf.price_sensitivity,
        event_category_preferences = tuf.event_category_preferences,
        last_updated_at = CURRENT_TIMESTAMP
    FROM temp_user_features tuf
    WHERE user_features.user_id = tuf.user_id;
    
    -- Insert new features
    INSERT INTO user_features 
        (user_id, activity_level, price_sensitivity, event_category_preferences, last_updated_at)
    SELECT 
        tuf.user_id, tuf.activity_level, tuf.price_sensitivity, 
        tuf.event_category_preferences, CURRENT_TIMESTAMP
    FROM temp_user_features tuf
    WHERE NOT EXISTS (
        SELECT 1 FROM user_features uf WHERE uf.user_id = tuf.user_id
    );
    
    -- Clean up
    DROP TABLE IF EXISTS temp_user_features;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION generate_training_dataset(
    dataset_name VARCHAR(100),
    description VARCHAR(500) DEFAULT NULL
) 
RETURNS UUID AS $$
DECLARE
    dataset_id UUID := gen_random_uuid();
    sample_count INT := 0;
    dataset_json TEXT;
BEGIN
    -- Create JSON array of samples (using PostgreSQL json functions)
    SELECT json_agg(
        json_build_object(
            'user_id', CAST(uei.user_id AS VARCHAR),
            'event_id', CAST(uei.event_id AS VARCHAR),
            'label', uei.interaction_strength
        )
    )
    INTO dataset_json
    FROM user_event_interactions uei;
    
    -- Count samples
    SELECT COUNT(*) INTO sample_count FROM user_event_interactions;
    
    -- Store the dataset
    INSERT INTO training_datasets (id, name, description, created_at, sample_count, is_active, dataset_json)
    VALUES (dataset_id, dataset_name, description, CURRENT_TIMESTAMP, sample_count, TRUE, dataset_json);
    
    -- Deactivate other datasets
    UPDATE training_datasets SET is_active = FALSE WHERE id <> dataset_id;
    
    -- Return the dataset ID
    RETURN dataset_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION run_data_sync() 
RETURNS TEXT AS $$
DECLARE
    dataset_name VARCHAR(100);
    result_message TEXT;
BEGIN
    -- Execute each function in sequence
    PERFORM sync_users();
    PERFORM sync_categories();
    PERFORM sync_events();
    PERFORM sync_orders();
    PERFORM sync_interactions();
    
    -- Update derived features
    PERFORM update_event_features();
    PERFORM update_user_features();
    
    -- Generate a new training dataset
    dataset_name := 'recommendation_dataset_' || to_char(CURRENT_DATE, 'YYYYMMDD');
    PERFORM generate_training_dataset(dataset_name, 'Auto-generated dataset');
    
    result_message := 'Data sync completed successfully';
    RETURN result_message;
END;
$$ LANGUAGE plpgsql;

-- Recommendation System Training Database Initialization Script
-- This script creates all tables and populates them with mock data (2025)

-- Insert mock data
-- Add categories
INSERT INTO categories (id, name) VALUES 
    (gen_random_uuid(), 'Concert'),
    (gen_random_uuid(), 'Sports'),
    (gen_random_uuid(), 'Theater'),
    (gen_random_uuid(), 'Festival'),
    (gen_random_uuid(), 'Conference'),
    (gen_random_uuid(), 'Comedy'),
    (gen_random_uuid(), 'Exhibition'),
    (gen_random_uuid(), 'Family & Kids');
    
-- Add users with direct UUID generation
INSERT INTO users (id, user_name, full_name, gender) VALUES
    (gen_random_uuid(), 'john.doe', 'John Doe', 0),
    (gen_random_uuid(), 'jane.smith', 'Jane Smith', 1),
    (gen_random_uuid(), 'alex.wilson', 'Alex Wilson', 0),
    (gen_random_uuid(), 'emily.johnson', 'Emily Johnson', 1),
    (gen_random_uuid(), 'michael.brown', 'Michael Brown', 0),
    (gen_random_uuid(), 'sarah.lee', 'Sarah Lee', 1),
    (gen_random_uuid(), 'david.wong', 'David Wong', 0),
    (gen_random_uuid(), 'olivia.garcia', 'Olivia Garcia', 1),
    (gen_random_uuid(), 'james.nguyen', 'James Nguyen', 0),
    (gen_random_uuid(), 'sophia.patel', 'Sophia Patel', 1),
    (gen_random_uuid(), 'ryan.moore', 'Ryan Moore', 0),
    (gen_random_uuid(), 'emma.kim', 'Emma Kim', 1),
    (gen_random_uuid(), 'tyler.jackson', 'Tyler Jackson', 0),
    (gen_random_uuid(), 'ava.martinez', 'Ava Martinez', 1),
    (gen_random_uuid(), 'noah.miller', 'Noah Miller', 2);

-- Create temporary tables to store IDs
CREATE TEMPORARY TABLE category_ids (category_name VARCHAR(100), category_id UUID);
CREATE TEMPORARY TABLE user_ids (user_name VARCHAR(100), user_id UUID);

-- Populate the temporary tables with the IDs we just created
INSERT INTO category_ids (category_name, category_id)
SELECT name, id FROM categories;

INSERT INTO user_ids (user_name, user_id)
SELECT user_name, id FROM users;

-- Now we can use these to create events
INSERT INTO events (id, category_id, created_by_id, name, location_name, city, district, ward, 
                   street_address, description, banner, start_date, end_date, event_slug, status) VALUES
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Concert'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'john.doe'), 
        'Summer Music Festival 2025', 'Central Park Arena', 'New York', 'Manhattan', 'Midtown', 
        '123 Main St', 'The biggest summer music festival of the year', 'banner1.jpg', 
        '2025-06-15', '2025-06-17', 'summer-music-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Theater'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'), 
        'Hamilton: 2025 Tour', 'Broadway Theater', 'New York', 'Manhattan', 'Theater District', 
        '222 Broadway Ave', 'Award-winning musical on national tour', 'hamilton.jpg', 
        '2025-03-20', '2025-04-15', 'hamilton-2025-tour', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Sports'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'), 
        'NBA Finals 2025', 'Madison Square Garden', 'New York', 'Manhattan', 'Midtown', 
        '4 Pennsylvania Plaza', 'Championship basketball games', 'nba2025.jpg', 
        '2025-05-25', '2025-06-10', 'nba-finals-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Festival'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'), 
        'Food & Wine Festival 2025', 'Javits Convention Center', 'New York', 'Manhattan', 'Hell''s Kitchen', 
        '429 11th Ave', 'Gourmet food and wine tasting event', 'foodfest.jpg', 
        '2025-09-12', '2025-09-14', 'food-wine-fest-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Conference'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'), 
        'Tech Innovation Summit 2025', 'Moscone Center', 'San Francisco', 'SoMa', 'Mission Bay', 
        '747 Howard St', 'Annual technology conference showcasing the latest innovations', 'techsummit.jpg', 
        '2025-10-05', '2025-10-07', 'tech-summit-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Comedy'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'sarah.lee'), 
        'Stand-up Comedy Night', 'Comedy Cellar', 'New York', 'Manhattan', 'Greenwich Village', 
        '117 MacDougal St', 'A night of laughs with top comedians', 'comedy.jpg', 
        '2025-02-14', '2025-02-14', 'comedy-night-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Concert'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'david.wong'), 
        'Taylor Swift: Eras Tour 2025', 'Allegiant Stadium', 'Las Vegas', 'Paradise', 'Strip', 
        '3333 Al Davis Way', 'The most anticipated tour of the year', 'swift2025.jpg', 
        '2025-07-21', '2025-07-23', 'swift-eras-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Exhibition'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'olivia.garcia'), 
        'Modern Art Exhibition', 'MoMA', 'New York', 'Manhattan', 'Midtown', 
        '11 W 53rd St', 'Showcasing contemporary artists from around the world', 'modernart.jpg', 
        '2025-04-10', '2025-08-15', 'modern-art-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Sports'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'james.nguyen'), 
        'US Open Tennis 2025', 'USTA Billie Jean King National Tennis Center', 'New York', 'Queens', 'Flushing', 
        'Flushing Meadows-Corona Park', 'Grand Slam tennis tournament', 'usopen.jpg', 
        '2025-08-25', '2025-09-07', 'us-open-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Festival'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'), 
        'International Film Festival', 'Lincoln Center', 'New York', 'Manhattan', 'Upper West Side', 
        '10 Lincoln Center Plaza', 'Celebrating cinema from around the world', 'filmfest.jpg', 
        '2025-11-01', '2025-11-15', 'film-festival-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Family & Kids'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'ryan.moore'), 
        'Disney On Ice 2025', 'Barclays Center', 'New York', 'Brooklyn', 'Prospect Heights', 
        '620 Atlantic Ave', 'Magical ice skating show for the whole family', 'disneyice.jpg', 
        '2025-12-20', '2025-12-28', 'disney-ice-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Conference'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'emma.kim'), 
        'Global Business Forum', 'Jacob K. Javits Convention Center', 'New York', 'Manhattan', 'Hell''s Kitchen', 
        '429 11th Ave', 'Leading business conference for executives', 'bizforum.jpg', 
        '2025-03-15', '2025-03-17', 'business-forum-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Concert'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'tyler.jackson'), 
        'Classical Symphony Night', 'Carnegie Hall', 'New York', 'Manhattan', 'Midtown', 
        '881 7th Ave', 'Evening of classical masterpieces', 'symphony.jpg', 
        '2025-05-10', '2025-05-10', 'symphony-night-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Theater'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'ava.martinez'), 
        'Broadway Show Collection', 'Various Theaters', 'New York', 'Manhattan', 'Theater District', 
        'Broadway & 7th Ave', 'Collection of the best Broadway shows', 'broadway.jpg', 
        '2025-01-15', '2025-12-31', 'broadway-collection-2025', 1),
    
    (gen_random_uuid(), (SELECT category_id FROM category_ids WHERE category_name = 'Exhibition'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'noah.miller'), 
        'Science & Technology Expo', 'American Museum of Natural History', 'New York', 'Manhattan', 'Upper West Side', 
        'Central Park West & 79th St', 'Interactive exhibition of scientific discoveries', 'scienceexpo.jpg', 
        '2025-06-01', '2025-08-31', 'science-expo-2025', 1);

-- Create a temporary table for event IDs
CREATE TEMPORARY TABLE event_ids (event_name VARCHAR(255), event_id UUID);

-- Populate the temporary table
INSERT INTO event_ids (event_name, event_id)
SELECT name, id FROM events;

-- Insert mock orders
INSERT INTO orders (id, event_id, user_ordered_id, total_price, total_quantity, created_at) VALUES
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'), 150.00, 2, '2025-01-05'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Hamilton: 2025 Tour'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'), 200.00, 1, '2025-02-10'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'john.doe'), 300.00, 2, '2025-03-12'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'), 75.00, 1, '2025-01-07'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Tech Innovation Summit 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'), 350.00, 1, '2025-04-16'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Stand-up Comedy Night'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'david.wong'), 80.00, 2, '2025-01-18'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'olivia.garcia'), 450.00, 2, '2025-03-22'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Modern Art Exhibition'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'james.nguyen'), 40.00, 1, '2025-02-28'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'US Open Tennis 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'), 250.00, 2, '2025-05-10'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'International Film Festival'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'ryan.moore'), 90.00, 1, '2025-06-15'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Disney On Ice 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'emma.kim'), 180.00, 3, '2025-07-20'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Global Business Forum'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'tyler.jackson'), 400.00, 1, '2025-08-25'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Classical Symphony Night'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'ava.martinez'), 120.00, 2, '2025-02-15'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Broadway Show Collection'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'noah.miller'), 280.00, 2, '2025-01-30'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Science & Technology Expo'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'john.doe'), 60.00, 2, '2025-06-05'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'), 225.00, 1, '2025-03-25'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'david.wong'), 350.00, 2, '2025-05-30'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Food & Wine Festival 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'), 90.00, 1, '2025-09-01'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'Modern Art Exhibition'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'emma.kim'), 40.00, 1, '2025-04-05'),
        
    (gen_random_uuid(), (SELECT event_id FROM event_ids WHERE event_name = 'US Open Tennis 2025'), 
        (SELECT user_id FROM user_ids WHERE user_name = 'tyler.jackson'), 125.00, 1, '2025-08-30');

-- Insert mock user event interactions (views, bookmarks, etc.)
INSERT INTO user_event_interactions (user_id, event_id, interaction_type, interaction_strength, interaction_count, last_interaction_at) VALUES
    -- Jane Smith views and purchases Summer Music Festival
    ((SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'),
     'View', 0.4, 3, '2025-01-01'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'),
     'Purchase', 1.0, 1, '2025-01-05'),
      
    -- Alex Wilson views Hamilton and NBA Finals, purchases Hamilton
    ((SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Hamilton: 2025 Tour'),
     'View', 0.5, 4, '2025-02-05'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Hamilton: 2025 Tour'),
     'Purchase', 1.0, 1, '2025-02-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'),
     'View', 0.3, 2, '2025-03-01'),
      
    -- John Doe views NBA Finals, purchases it
    ((SELECT user_id FROM user_ids WHERE user_name = 'john.doe'),
     (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'),
     'View', 0.6, 5, '2025-03-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'john.doe'),
     (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'),
     'Purchase', 1.0, 1, '2025-03-12'),
      
    -- Emily Johnson views and bookmarks several events
    ((SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'),
     'View', 0.3, 2, '2025-01-06'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Summer Music Festival 2025'),
     'Purchase', 1.0, 1, '2025-01-07'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'View', 0.4, 3, '2025-03-01'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'Bookmark', 0.7, 1, '2025-03-02'),
      
    -- Michael Brown has specific interests
    ((SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Tech Innovation Summit 2025'),
     'View', 0.6, 6, '2025-04-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Tech Innovation Summit 2025'),
     'Purchase', 1.0, 1, '2025-04-16'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Science & Technology Expo'),
     'View', 0.5, 4, '2025-05-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Global Business Forum'),
     'View', 0.3, 2, '2025-03-05'),
      
    -- Add 20 more interactions for different users and events
    ((SELECT user_id FROM user_ids WHERE user_name = 'david.wong'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Stand-up Comedy Night'),
     'View', 0.5, 4, '2025-01-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'david.wong'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Stand-up Comedy Night'),
     'Purchase', 1.0, 1, '2025-01-18'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'olivia.garcia'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'View', 0.6, 7, '2025-03-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'olivia.garcia'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'Purchase', 1.0, 1, '2025-03-22'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'james.nguyen'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Modern Art Exhibition'),
     'View', 0.4, 3, '2025-02-25'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'james.nguyen'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Modern Art Exhibition'),
     'Purchase', 1.0, 1, '2025-02-28'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'),
     (SELECT event_id FROM event_ids WHERE event_name = 'International Film Festival'),
     'View', 0.5, 5, '2025-10-25'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'),
     (SELECT event_id FROM event_ids WHERE event_name = 'US Open Tennis 2025'),
     'View', 0.4, 4, '2025-05-05'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'sophia.patel'),
     (SELECT event_id FROM event_ids WHERE event_name = 'US Open Tennis 2025'),
     'Purchase', 1.0, 1, '2025-05-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'ryan.moore'),
     (SELECT event_id FROM event_ids WHERE event_name = 'International Film Festival'),
     'View', 0.3, 2, '2025-06-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'ryan.moore'),
     (SELECT event_id FROM event_ids WHERE event_name = 'International Film Festival'),
     'Purchase', 1.0, 1, '2025-06-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emma.kim'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Disney On Ice 2025'),
     'View', 0.5, 5, '2025-07-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emma.kim'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Disney On Ice 2025'),
     'Purchase', 1.0, 1, '2025-07-20'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'tyler.jackson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Global Business Forum'),
     'View', 0.6, 6, '2025-08-20'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'tyler.jackson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Global Business Forum'),
     'Purchase', 1.0, 1, '2025-08-25'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'ava.martinez'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Classical Symphony Night'),
     'View', 0.4, 3, '2025-02-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'ava.martinez'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Classical Symphony Night'),
     'Purchase', 1.0, 1, '2025-02-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'noah.miller'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Broadway Show Collection'),
     'View', 0.5, 4, '2025-01-25'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'noah.miller'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Broadway Show Collection'),
     'Purchase', 1.0, 1, '2025-01-30'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'john.doe'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Science & Technology Expo'),
     'View', 0.3, 2, '2025-06-01'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'john.doe'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Science & Technology Expo'),
     'Purchase', 1.0, 1, '2025-06-05'),
      
    -- Add view interactions for events that weren't purchased
    ((SELECT user_id FROM user_ids WHERE user_name = 'john.doe'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'View', 0.2, 1, '2025-07-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'jane.smith'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Classical Symphony Night'),
     'View', 0.3, 2, '2025-04-20'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'alex.wilson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Disney On Ice 2025'),
     'View', 0.1, 1, '2025-11-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'emily.johnson'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Science & Technology Expo'),
     'View', 0.4, 3, '2025-06-20'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'michael.brown'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Taylor Swift: Eras Tour 2025'),
     'View', 0.1, 1, '2025-07-10'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'sarah.lee'),
     (SELECT event_id FROM event_ids WHERE event_name = 'NBA Finals 2025'),
     'View', 0.2, 1, '2025-05-15'),
      
    ((SELECT user_id FROM user_ids WHERE user_name = 'david.wong'),
     (SELECT event_id FROM event_ids WHERE event_name = 'Food & Wine Festival 2025'),
     'View', 0.3, 2, '2025-09-01');

-- Generate event features based on interactions
INSERT INTO event_features (event_id, popularity, avg_ticket_price, view_count, purchase_count, last_updated_at)
SELECT 
    e.id,
    CASE
        WHEN COUNT(uei.id) = 0 THEN 0.1 -- Some baseline popularity
        ELSE 0.1 + (0.9 * COUNT(uei.id) / 10.0) -- Normalized to max of 1.0
    END AS popularity,
    COALESCE(AVG(o.total_price / o.total_quantity), 50.00) AS avg_ticket_price,
    SUM(CASE WHEN uei.interaction_type = 'View' THEN uei.interaction_count ELSE 0 END) AS view_count,
    SUM(CASE WHEN uei.interaction_type = 'Purchase' THEN uei.interaction_count ELSE 0 END) AS purchase_count,
    CURRENT_TIMESTAMP AS last_updated_at
FROM 
    events e
LEFT JOIN 
    user_event_interactions uei ON e.id = uei.event_id
LEFT JOIN 
    orders o ON e.id = o.event_id
GROUP BY 
    e.id;
    
-- Generate user features based on interactions
INSERT INTO user_features (user_id, activity_level, price_sensitivity, event_category_preferences, last_updated_at)
SELECT 
    u.id AS user_id,
    CASE
        WHEN COUNT(uei.id) = 0 THEN 0.1
        ELSE 0.1 + (0.9 * COUNT(uei.id) / 10.0) -- Normalize to 0-1
    END AS activity_level,
    CASE
        WHEN AVG(o.total_price) IS NULL THEN 0.5 -- Default mid sensitivity
        WHEN AVG(o.total_price) > 200 THEN 0.2   -- Less sensitive to price (buys expensive tickets)
        WHEN AVG(o.total_price) > 100 THEN 0.5   -- Medium sensitivity
        ELSE 0.8                                -- High sensitivity (prefers cheaper tickets)
    END AS price_sensitivity,
    NULL AS event_category_preferences,
    CURRENT_TIMESTAMP AS last_updated_at
FROM 
    users u
LEFT JOIN 
    user_event_interactions uei ON u.id = uei.user_id
LEFT JOIN 
    orders o ON u.id = o.user_ordered_id
GROUP BY 
    u.id;

-- Create a sample training dataset from interactions
INSERT INTO training_datasets (id, name, description, created_at, sample_count, is_active, dataset_json)
SELECT
    gen_random_uuid() AS id,
    'initial_training_dataset_2025' AS name,
    'Automatically generated initial training data with 2025 events' AS description,
    CURRENT_TIMESTAMP AS created_at,
    (SELECT COUNT(*) FROM user_event_interactions) AS sample_count,
    TRUE AS is_active,
    NULL AS dataset_json;

-- Clean up temporary tables
DROP TABLE category_ids;
DROP TABLE user_ids;
DROP TABLE event_ids;

SELECT 'Training database initialized successfully with 2025 mock data.' AS initialization_complete;