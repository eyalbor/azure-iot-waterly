import _ from 'lodash'
import {
    QUALITY_DEVICE,
} from '../actions/types'

export default (state = {}, action ) => {
    switch(action.type) {
        case QUALITY_DEVICE:
            return {...state, [action.payload.id]: action.payload};
        default:
            return state;
    }
};