from typing import List


class ParetoFrontFinder:
    def __init__(self):
        self.front = []
        self.pending_front = []

    def push(self, candidate):
        will_add = True

        for existing in self.front:
            domination_result = who_dominates(existing, candidate)

            if domination_result == 0:
                self.pending_front.append(existing)

            elif domination_result < 0:
                self.pending_front.append(existing)
                will_add = False

        if will_add:
            self.pending_front.append(candidate)

        self.front, self.pending_front = self.pending_front, self.front
        self.pending_front.clear()


def who_dominates(vec1, vec2):
    better_count = 0
    worse_count = 0

    for i in range(0, len(vec1)):
        val1, val2 = vec1[i], vec2[i]

        if val1 == val2:
            continue

        if val2 > val1:
            better_count += 1
        else:
            worse_count += 1

        if better_count > 0 and worse_count > 0:
            return 0

    if better_count > 0 and worse_count == 0:
        return 1

    if worse_count > 0 and better_count == 0:
        return -1

    return 0
