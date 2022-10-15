import numpy as np
import csconnector as csc

csc.init_environment()
input = csc.get_input()
input = np.array(input)

input.sort()
input += input

csc.set_output(input.tolist())
