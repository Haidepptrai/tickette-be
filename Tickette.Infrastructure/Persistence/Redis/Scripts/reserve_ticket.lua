-- reserve_multiple_tickets.lua

-- KEYS = array of inventory keys and reservation keys: [inv1, res1, inv2, res2, ...]
-- ARGV = [userId, timestamp, qty1, qty2, ..., qtyN]

local userId = ARGV[1]
local timestamp = ARGV[2]

local count = (#KEYS / 2)
for i = 1, count do
    local invKey = KEYS[i * 2 - 1]
    local resKey = KEYS[i * 2]

    local newQty = tonumber(ARGV[2 + i])
    local stock = tonumber(redis.call('GET', invKey))
    local oldQty = 0

    if redis.call('EXISTS', resKey) == 1 then
        oldQty = tonumber(redis.call('HGET', resKey, 'quantity')) or 0
    end

    local diff = newQty - oldQty

    if stock == nil then
        return {-1, i}  -- Inventory key not found at index i
    end

    if stock < diff then
        return {-2, i}  -- Not enough inventory at index i
    end

    redis.call('DECRBY', invKey, diff)
    redis.call('HSET', resKey,
        'quantity', newQty,
        'user_id', userId,
        'reserved_at', timestamp
    )
end

return 1  -- All successful
