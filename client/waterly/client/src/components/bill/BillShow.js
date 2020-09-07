import React from 'react'
import { connect } from 'react-redux'
import { fetchBills, payForBill } from '../../actions'
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from 'reactstrap';
import Paper from '@material-ui/core/Paper';
import {
    Chart,
    Title,
    CommonSeriesSettings,
    Series,
    Legend,
    Font,
    Size
  } from 'devextreme-react/chart';

import { withStyles } from '@material-ui/core/styles';

const legendStyles = () => ({
    root: {
        display: 'flex',
        margin: 'auto',
        flexDirection: 'row'
    },
});

const legendRootBase = ({ classes, ...restProps }) => (
    <Legend.Root {...restProps} className={classes.root} />
);
const Root = withStyles(legendStyles, { name: 'LegendRoot' })(legendRootBase);
const legendLabelStyles = () => ({
    label: {
        whiteSpace: 'nowrap',
}   ,
});
const legendLabelBase = ({ classes, ...restProps }) => (
    <Legend.Label className={classes.label} {...restProps} />
);
const Label = withStyles(legendLabelStyles, { name: 'LegendLabel' })(legendLabelBase);

class BillShow extends React.Component {

    constructor(props) {
        super(props)
        this.state = {
            modal: false,
            bill: null
        }
    }

    componentDidMount(){
        this.props.fetchBills();
    }

    toggle = (bill) => {
        console.log(bill)
        this.setState({modal: !this.state.modal, bill: bill})
    }

    showPayButton(bill){
        if(!bill.status){
            return(
                <div className="extra content">
                    <button className="ui primary button" onClick={()=>this.toggle(bill)}>Pay</button>
                </div>
            );
        } else{
            return(
                <div className="extra content">
                    <button className="ui disabled button">Paid</button>
                </div>
            )
        }
    }

    pay = (bill) => {
        console.log('pay')
        console.log(bill)
        bill.status = true
        this.props.payForBill(bill);
        this.toggle()
    }

    renderCards(){
        //console.log(this.props.bills)
        return this.props.bills.map(bill => {
            return (     
                <div className="ui card" key={bill.id} style={{padding: "10px", margin:"10px"}}>
                    <div className="content">
                        <div className="header"> {bill.month} {bill.year}</div>
                        <div className="meta">
                            Total flow: {bill.total_flow} m³/s
                            <br/>
                            Water expenses: {bill.water_expenses} ILS
                            <br/>
                            Fixed expenses: {bill.fixed_expenses} ILS
                        </div>
                        <div className="description">
                            Total price: {bill.water_expenses+bill.fixed_expenses} ILS
                        </div>      
                    </div>
                    <div className="summary">
                        <Chart id="chart" dataSource={[
                        {
                            name: "",
                            user: bill.water_expenses+bill.fixed_expenses,
                            avg: bill.avg
                        }
                        ]}>
                        <Size
                            height={300}
                            width={250}
                        />
                        <Title text="Total Consumption [m³/s]">
                            <Font size="14"/>
                        </Title>
                        <CommonSeriesSettings
                            argumentField="name"
                            type="bar"
                            ignoreEmptyPoints={true}
                            />
                            <Series valueField="user" name="Your Consumption"/>
                            <Series valueField="avg" name="Average Consumption"/>
                            <Legend verticalAlignment="bottom" horizontalAlignment="center" />
                        </Chart>
                    </div>
                    {this.showPayButton(bill)}
                </div>
            )
        })
    }

    render(){
        return (
            <>
                <div className="ui container">
                    <div className="ui special cards">
                        {this.renderCards()}
                    </div>
                </div>

                <Modal isOpen={this.state.modal} toggle={this.toggle}>
                    <ModalHeader toggle={this.toggle}>Bill Payment</ModalHeader>
                    <ModalBody>
                        You are about to pay your water bill.<br/>
                        The total is {this.state.modal? this.state.bill.water_expenses+this.state.bill.fixed_expenses:''}
                    </ModalBody>
                    <ModalFooter>
                        <Button color="primary" onClick={() => this.pay(this.state.bill)}>Confirm</Button>{' '}
                        <Button color="secondary" onClick={this.toggle}>Cancel</Button>
                    </ModalFooter>
                </Modal>
            </>
        )
    }
  
};

const mapStateToProps = (state) => {
    return {
        bills: Object.values(state.bills),
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, {fetchBills, payForBill})(BillShow);