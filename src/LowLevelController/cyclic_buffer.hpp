// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef CYCLIC_BUFFER_HPP
#define CYCLIC_BUFFER_HPP

#include <cstddef> // For size_t
#include <atomic>  // For std::atomic

template <typename T, size_t Size>
class CyclicBuffer
{
public:
    CyclicBuffer() : _head(0), _tail(0) {
        static_assert((Size & (Size - 1)) == 0, "Size must be a power of two");
    }

    // Pushes an item into the buffer (thread-safe for single producer)
    void push(const T& item) {
        const auto current_head = _head.load(std::memory_order_relaxed);
        _buffer[current_head] = item;
        _head.store((current_head + 1) & (Size - 1), std::memory_order_release);
    }

    // Pops an item from the buffer (thread-safe for single consumer)
    bool pop(T& item) {
        const auto current_tail = _tail.load(std::memory_order_relaxed);
        if (current_tail == _head.load(std::memory_order_acquire)) {
            return false; // Buffer is empty
        }
        item = _buffer[current_tail];
        _tail.store((current_tail + 1) & (Size - 1), std::memory_order_release);
        return true;
    }

    bool is_empty() const {
        return _head.load(std::memory_order_acquire) == _tail.load(std::memory_order_acquire);
    }

    bool is_full() const {
        return ((_head.load(std::memory_order_acquire) + 1) & (Size - 1)) == _tail.load(std::memory_order_acquire);
    }

private:
    T _buffer[Size];
    alignas(4) std::atomic<size_t> _head;
    alignas(4) std::atomic<size_t> _tail;
};


#endif // CYCLIC_BUFFER_HPP
