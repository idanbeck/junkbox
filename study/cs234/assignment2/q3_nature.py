import tensorflow as tf
import tensorflow.contrib.layers as layers

from utils.general import get_logger
from utils.test_env import EnvTest
from q1_schedule import LinearExploration, LinearSchedule
from q2_linear import Linear


from configs.q3_nature import config


class NatureQN(Linear):
    """
    Implementing DeepMind's Nature paper. Here are the relevant urls.
    https://storage.googleapis.com/deepmind-data/assets/papers/DeepMindNature14236Paper.pdf
    https://www.cs.toronto.edu/~vmnih/docs/dqn.pdf
    """
    def get_q_values_op(self, state, scope, reuse=False):
        """
        Returns Q values for all actions

        Args:
            state: (tf tensor) 
                shape = (batch_size, img height, img width, nchannels)
            scope: (string) scope name, that specifies if target network or not
            reuse: (bool) reuse of variables in the scope

        Returns:
            out: (tf tensor) of shape = (batch_size, num_actions)
        """
        # this information might be useful
        num_actions = self.env.action_space.n

        ##############################################################
        """
        TODO: implement the computation of Q values like in the paper
                https://storage.googleapis.com/deepmind-data/assets/papers/DeepMindNature14236Paper.pdf
                https://www.cs.toronto.edu/~vmnih/docs/dqn.pdf

              you may find the section "model architecture" of the appendix of the 
              nature paper particulary useful.

              store your result in out of shape = (batch_size, num_actions)

        HINT: 
            - You may find the following functions useful:
                - tf.layers.conv2d
                - tf.layers.flatten
                - tf.layers.dense

            - Make sure to also specify the scope and reuse

        """
        ##############################################################
        ################ YOUR CODE HERE - 10-15 lines ################ 

        with tf.variable_scope(scope, reuse=reuse) as var_scope:
            conv_1 = layers.conv2d(state, 32, 8, 4, activation_fn=None)
            conv_1_relu = tf.nn.relu(conv_1)

            conv_2 = layers.conv2d(conv_1_relu, 64, 4, 2, activation_fn=None)
            conv_2_relu = tf.nn.relu(conv_2)

            conv_3 = layers.conv2d(conv_2_relu, 64, 3, 1, activation_fn=None)
            conv_3_relu = tf.nn.relu(conv_3)

            flat = layers.flatten(conv_3_relu)
            FC = layers.fully_connected(flat, 512, activation_fn=None)
            FC_relu = tf.nn.relu(FC)

            out = layers.fully_connected(FC_relu, num_actions, activation_fn=None, reuse=reuse, scope=scope)

        ##############################################################
        ######################## END YOUR CODE #######################
        return out


"""
Use deep Q network for test environment.
"""
if __name__ == '__main__':
    env = EnvTest((80, 80, 1))

    # exploration strategy
    exp_schedule = LinearExploration(env, config.eps_begin, 
            config.eps_end, config.eps_nsteps)

    # learning rate schedule
    lr_schedule  = LinearSchedule(config.lr_begin, config.lr_end,
            config.lr_nsteps)

    # train model
    model = NatureQN(env, config)
    model.run(exp_schedule, lr_schedule)
