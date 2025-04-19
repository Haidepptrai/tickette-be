-- reserve_ticket_with_seats.lua

    -- KEYS[1] = inventory key
    -- KEYS[2] = reservation key
    -- Remaining KEYS: dynamic seat keys
    -- ARGV[1] = new quantity
    -- ARGV[2] = user ID
    -- ARGV[3] = reserved_at timestamp
    -- ARGV[4] = seat count (N)
    -- ARGV[5..(5+N-1)] = seat keys to check (reserved + booked)
    -- ARGV[(5+N)..] = flattened key-value pairs to write

local stock = tonumber(redis.call('GET', KEYS[1]))
local newQty = tonumber(ARGV[1])
local userId = ARGV[2]
local timestamp = ARGV[3]
local seatCount = tonumber(ARGV[4])
local oldQty = 0

    -- Read old reservation quantity
if redis.call('EXISTS', KEYS[2]) == 1 then
    oldQty = tonumber(redis.call('HGET', KEYS[2], 'quantity')) or 0
end

local diff = newQty - oldQty
if stock == nil then return -1 end
if stock < diff then return -2 end

    -- Validate all seats
local conflictSeats = {}
for i = 1, seatCount do
    local seatKey = ARGV[4 + i]
    if redis.call('EXISTS', seatKey) == 1 then
        table.insert(conflictSeats, seatKey)
    end
end

if #conflictSeats > 0 then
    return conflictSeats
end


    -- All checks passed, reserve seats + update inventory
redis.call('DECRBY', KEYS[1], diff)
redis.call('HSET', KEYS[2], 'quantity', newQty, 'user_id', userId, 'reserved_at', timestamp)

    -- Set seat reservations
local offset = 4 + seatCount
for i = offset + 1, #ARGV, 3 do
    local key = ARGV[i]
local field = ARGV[i + 1]
local value = ARGV[i + 2]
redis.call('HSET', key, field, value)
end

return 1