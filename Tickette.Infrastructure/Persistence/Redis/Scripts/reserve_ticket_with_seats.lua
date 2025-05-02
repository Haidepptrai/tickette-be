-- reserve_multiple_tickets_with_seats.lua

-- KEYS = inventory/reservation keys + all seat keys
-- ARGV = [userId, timestamp, ticketCount,
--         seatCount1, qty1, seatKeys1..., writeArgs1...,
--         seatCount2, qty2, seatKeys2..., writeArgs2..., ...]

local userId = ARGV[1]
local timestamp = ARGV[2]
local ticketCount = tonumber(ARGV[3])

local index = 4
local conflictSeats = {}

for i = 1, ticketCount do
    local invKey = KEYS[i * 2 - 1]
    local resKey = KEYS[i * 2]
    local seatCount = tonumber(ARGV[index])
    local newQty = tonumber(ARGV[index + 1])
    local oldQty = 0

    index = index + 2

    -- Read old quantity
    if redis.call('EXISTS', resKey) == 1 then
        oldQty = tonumber(redis.call('HGET', resKey, 'quantity')) or 0
    end

    local diff = newQty - oldQty
    local stock = tonumber(redis.call('GET', invKey))

    if stock == nil then return {-1, i} end
    if stock < diff then return {-2, i} end

    -- Check seat conflicts
    for j = 1, seatCount do
        local seatKey = ARGV[index]
        if redis.call('EXISTS', seatKey) == 1 then
            table.insert(conflictSeats, seatKey)
        end
        index = index + 1
    end

    -- HSET reservation
    redis.call('HSET', resKey,
        'quantity', newQty,
        'user_id', userId,
        'reserved_at', timestamp
    )

    -- Update inventory
    redis.call('DECRBY', invKey, diff)

    -- HSET for seat reservations (3 at a time: key, field, value)
    local writeCount = tonumber(ARGV[index])
    index = index + 1
    for w = 1, writeCount do
        local seatHKey = ARGV[index]
        local field = ARGV[index + 1]
        local value = ARGV[index + 2]
        redis.call('HSET', seatHKey, field, value)
        index = index + 3
    end
end

if #conflictSeats > 0 then
    return conflictSeats
end

return 1
