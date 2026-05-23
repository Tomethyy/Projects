"""Roll one or more six-sided dice and print the results."""

import random


def roll_dice(count: int = 2, sides: int = 6) -> list[int]:
    return [random.randint(1, sides) for _ in range(count)]


def main() -> None:
    try:
        count = int(input("How many dice? (default 2): ") or "2")
        sides = int(input("How many sides per die? (default 6): ") or "6")
    except ValueError:
        print("Please enter whole numbers.")
        return

    if count < 1 or sides < 2:
        print("Need at least 1 die with 2 or more sides.")
        return

    rolls = roll_dice(count, sides)
    print(f"Rolled: {rolls}")
    print(f"Total: {sum(rolls)}")


if __name__ == "__main__":
    main()
