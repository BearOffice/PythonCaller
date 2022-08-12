import numpy as np
import csconnector as csc

input = csc.get_input()
input = np.array(input)

input.sort()
input += input

csc.set_output(list(input))
