import numpy


def read_array_from_file(array_path):
    array = {}
    width = 0
    height = 0
    k = 0
    t = True

    for line in open(array_path, "r").read().split("\n"):
        height = height + 1
        for unit in line.split(" "):
            if unit != "":
                array[k] = float(unit)
                k = k + 1
                if t:
                    width = width + 1
            else:
                t = False

    height = height - 1
    symbol = numpy.zeros(shape=(width, height), dtype=numpy.float)

    for i in range(0, height):
        for j in range(0, width):
            symbol[i, j] = array[i * height + j]

    return symbol
