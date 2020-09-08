import React from 'react'
import ReactSpeedometer from "react-d3-speedometer"
import Paper from '@material-ui/core/Paper';

const SpeedometerPressure = props => {
    return (
    <div className="ui center aligned compact segment">
        <h4 class="ui header">Current Avarege Pressure</h4>
        <ReactSpeedometer
            maxValue={7}
            minValue={1}
            width={400}
            needleHeightRatio={0.6}
            value={props.avg}
            currentValueText={`Pressure: ${props.avg} atm` }
            segments={6}
            customSegmentStops={[1,2,3,4,5,6,7]}
            startColor="green"
            endColor="blue"
            ringWidth={47}
            needleTransitionDuration={3333}
            needleTransition="easeElastic"
            needleColor={"#90f2ff"}
        />
    </div>
    )
}

export default SpeedometerPressure;