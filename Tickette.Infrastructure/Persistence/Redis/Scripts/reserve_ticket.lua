-- reserve_ticket.lua
-- This script adjusts ticket inventory atomically when creating or updating a reservation

-- KEYS[1] = inventory key (e.g., Tickette:ticket:{ticketId}:remaining_tickets)
-- KEYS[2] = reservation key (e.g., Tickette:reservation:{ticketId}:{userId})

-- ARGV[1] = new requested quantity
-- ARGV[2] = user_id (for reference, not strictly needed by Redis)
-- ARGV[3] = timestamp

local stock = tonumber(redis.call('GET', KEYS[1]))
local newQty = tonumber(ARGV[1])
local oldQty = 0

-- If the reservation already exists, get the old quantity
if redis.call('EXISTS', KEYS[2]) == 1 then
    oldQty = tonumber(redis.call('HGET', KEYS[2], 'quantity')) or 0
end

-- Calculate how much inventory to adjust by
local diff = newQty - oldQty

-- Validate stock availability
if stock == nil then
    return -1  -- Inventory key does not exist
end

if stock < diff then
    return -2  -- Not enough inventory
end

-- Adjust the inventory
redis.call('DECRBY', KEYS[1], diff)

-- Save or update the reservation
redis.call('HSET', KEYS[2],
    'quantity', newQty,
    'user_id', ARGV[2],
    'reserved_at', ARGV[3]
)

return 1  -- Success
